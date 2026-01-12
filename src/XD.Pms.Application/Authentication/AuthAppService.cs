using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using Volo.Abp.Timing;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Authentication;

[RemoteService(IsEnabled = false)]
public class AuthAppService : PmsAppService, IAuthAppService
{
	private readonly IdentityUserManager _userManager;
	private readonly IJwtTokenGenerator _jwtTokenGenerator;
	private readonly IRefreshTokenRepository _refreshTokenRepository;
	private readonly JwtSettings _jwtSettings;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IIdentityRoleRepository _roleRepository;
	private readonly IClock _clock;

	public AuthAppService(
		IdentityUserManager userManager,
		IJwtTokenGenerator jwtTokenGenerator,
		IRefreshTokenRepository refreshTokenRepository,
		IOptions<JwtSettings> jwtSettings,
		IHttpContextAccessor httpContextAccessor,
		IIdentityRoleRepository roleRepository,
		IClock clock)
	{
		_userManager = userManager;
		_jwtTokenGenerator = jwtTokenGenerator;
		_refreshTokenRepository = refreshTokenRepository;
		_jwtSettings = jwtSettings.Value;
		_httpContextAccessor = httpContextAccessor;
		_roleRepository = roleRepository;
		_clock = clock;
	}

	/// <summary>
	/// 用户登录
	/// </summary>
	public async Task<LoginResponseDto> LoginAsync(LoginRequestDto input)
	{
		if (!Enum.IsDefined(input.TokenType))
		{
			throw new UserFriendlyException($"无效的TokenType值: {input.TokenType}", "400");
		}

		// 1. 查找用户
		var user = await _userManager.FindByNameAsync(input.UserNameOrEmail)
				?? await _userManager.FindByEmailAsync(input.UserNameOrEmail)
				?? throw new UserFriendlyException("当前账号不存在", code: "400");

		// 2. 验证用户状态
		if (!user.IsActive)
		{
			throw new UserFriendlyException("账户尚未启用", code: "403");
		}

		if (await _userManager.IsLockedOutAsync(user))
		{
			throw new UserFriendlyException("账户已被锁定，请稍后再试", code: "403");
		}

		// 3. 验证密码
		var passwordValid = await _userManager.CheckPasswordAsync(user, input.Password);
		if (!passwordValid)
		{
			await _userManager.AccessFailedAsync(user);
			throw new UserFriendlyException("用户名或密码错误", code: "400");
		}

		// 4. 重置登录失败次数
		await _userManager.ResetAccessFailedCountAsync(user);

		// 5. 获取用户角色
		var roles = await _userManager.GetRolesAsync(user);

		// 6. 生成 Access Token
		var (accessToken, accessTokenExpiration) = _jwtTokenGenerator.GenerateAccessToken(
			user.Id,
			user.UserName!,
			user.Email,
			roles,
			CurrentTenant.Id
		);

		// 7. 生成 Refresh Token
		var refreshTokenExpiration = input.RememberMe
			? _clock.Now.AddDays(_jwtSettings.RememberMeRefreshTokenExpirationDays)
			: _clock.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

		var refreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();

		// 8. 获取客户端信息
		var httpContext = _httpContextAccessor.HttpContext;
		var clientIp = GetClientIpAddress(httpContext);
		var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

		// 9. 检查并清理过多的活跃Token
		await CleanupExcessTokensAsync(user.Id, input.DeviceId);

		// 10. 保存 Refresh Token
		var refreshToken = new RefreshToken(
			GuidGenerator.Create(),
			user.Id,
			refreshTokenValue,
			refreshTokenExpiration,
			input.TokenType,
			clientIp,
			userAgent?.Length > 512 ? userAgent.Substring(0, 512) : userAgent,
			input.DeviceId,
			CurrentTenant.Id
		);

		await _refreshTokenRepository.InsertAsync(refreshToken);

		// 11. 返回结果
		return new LoginResponseDto
		{
			AccessToken = accessToken,
			RefreshToken = refreshTokenValue,
			TokenType = "Bearer",
			ExpiresIn = (int)(accessTokenExpiration - _clock.Now).TotalSeconds,
			AccessTokenExpiration = accessTokenExpiration,
			RefreshTokenExpiration = refreshTokenExpiration,
			User = new UserInfoDto
			{
				Id = user.Id,
				UserName = user.UserName!,
				Email = user.Email,
				Name = user.Name,
				Surname = user.Surname,
				PhoneNumber = user.PhoneNumber,
				Roles = [.. roles]
			}
		};
	}

	/// <summary>
	/// 刷新令牌
	/// </summary>
	public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input)
	{
		// 1. 查找 Refresh Token
		var storedToken = await _refreshTokenRepository.FindByTokenAsync(input.RefreshToken) 
				?? throw new UserFriendlyException("无效的刷新令牌");

		// 2. 验证 Token 状态
		if (storedToken.IsRevoked)
		{
			// 检测到令牌重用，撤销该用户所有令牌（安全措施）
			await _refreshTokenRepository.RevokeAllByUserIdAsync(storedToken.UserId);
			throw new UserFriendlyException("令牌已被撤销，请重新登录");
		}

		if (storedToken.ExpiresAt < _clock.Now)
		{
			throw new UserFriendlyException("刷新令牌已过期，请重新登录");
		}

		// 3. 获取用户信息
		var user = await _userManager.FindByIdAsync(storedToken.UserId.ToString());
		if (user == null || !user.IsActive)
		{
			await _refreshTokenRepository.RevokeAllByUserIdAsync(storedToken.UserId);
			throw new UserFriendlyException("用户不存在或已被禁用");
		}

		// 4. 获取用户角色
		var roles = await _userManager.GetRolesAsync(user);

		// 5. 生成新的 Access Token
		var (accessToken, accessTokenExpiration) = _jwtTokenGenerator.GenerateAccessToken(
			user.Id,
			user.UserName!,
			user.Email,
			roles,
			storedToken.TenantId
		);

		// 6. 实现 Refresh Token 轮换
		var newRefreshTokenValue = _jwtTokenGenerator.GenerateRefreshToken();
		var newRefreshTokenExpiration = _clock.Now.AddDays(_jwtSettings.RefreshTokenExpirationDays);

		var httpContext = _httpContextAccessor.HttpContext;
		var clientIp = GetClientIpAddress(httpContext);
		var userAgent = httpContext?.Request.Headers["User-Agent"].ToString();

		var newRefreshToken = new RefreshToken(
			GuidGenerator.Create(),
			user.Id,
			newRefreshTokenValue,
			newRefreshTokenExpiration,
			storedToken.TokenType,
			clientIp,
			userAgent?.Length > 512 ? userAgent[..512] : userAgent,
			storedToken.DeviceId,
			storedToken.TenantId
		);

		// 7. 撤销旧 Token，关联到新 Token
		storedToken.Revoke(_clock.Now, newRefreshToken.Id);
		await _refreshTokenRepository.UpdateAsync(storedToken);

		// 8. 保存新 Token
		await _refreshTokenRepository.InsertAsync(newRefreshToken);

		return new TokenResponseDto
		{
			AccessToken = accessToken,
			RefreshToken = newRefreshTokenValue,
			TokenType = "Bearer",
			ExpiresIn = (int)(accessTokenExpiration - _clock.Now).TotalSeconds,
			AccessTokenExpiration = accessTokenExpiration,
			RefreshTokenExpiration = newRefreshTokenExpiration
		};
	}

	/// <summary>
	/// 撤销令牌（登出）
	/// </summary>
	[Authorize]
	public async Task RevokeTokenAsync(RevokeTokenRequestDto input)
	{
		var userId = CurrentUser.Id!.Value;

		if (input.RevokeAll)
		{
			// 撤销所有设备的令牌
			await _refreshTokenRepository.RevokeAllByUserIdAsync(userId);
		}
		else if (!string.IsNullOrEmpty(input.RefreshToken))
		{
			// 撤销指定令牌
			var token = await _refreshTokenRepository.FindByTokenAsync(input.RefreshToken);
			if (token != null && token.UserId == userId)
			{
				token.Revoke(_clock.Now);
				await _refreshTokenRepository.UpdateAsync(token);
			}
		}
	}

	/// <summary>
	/// 获取当前用户信息
	/// </summary>
	[Authorize]
	public async Task<UserInfoDto> GetCurrentUserAsync()
	{
		var userId = CurrentUser.Id!.Value;
		var user = await _userManager.FindByIdAsync(userId.ToString()) ?? throw new UserFriendlyException("用户不存在");
		var roles = await _userManager.GetRolesAsync(user);

		return new UserInfoDto
		{
			Id = user.Id,
			UserName = user.UserName!,
			Email = user.Email,
			Name = user.Name,
			Surname = user.Surname,
			PhoneNumber = user.PhoneNumber,
			Roles = [.. roles]
		};
	}

	/// <summary>
	/// 获取用户活跃会话列表
	/// </summary>
	[Authorize]
	public async Task<List<UserSessionDto>> GetActiveSessionsAsync()
	{
		var userId = CurrentUser.Id!.Value;
		var tokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

		// 获取当前请求的 Token 信息以标识当前会话
		var currentRefreshToken = GetCurrentRefreshTokenFromHeader();

		return [.. tokens.Select(t => new UserSessionDto
		{
			TokenId = t.Id,
			DeviceId = t.DeviceId,
			ClientIp = t.ClientIp,
			UserAgent = t.UserAgent,
			TokenType = t.TokenType,
			CreatedAt = t.CreationTime,
			ExpiresAt = t.ExpiresAt,
			IsCurrent = t.Token == currentRefreshToken
		})];
	}

	/// <summary>
	/// 撤销指定会话
	/// </summary>
	[Authorize]
	public async Task RevokeSessionAsync(Guid tokenId)
	{
		var userId = CurrentUser.Id!.Value;
		var token = await _refreshTokenRepository.GetAsync(tokenId);

		if (token.UserId != userId)
		{
			throw new UserFriendlyException("无权操作此会话");
		}

		token.Revoke(_clock.Now);
		await _refreshTokenRepository.UpdateAsync(token);
	}

	#region 私有方法

	private async Task CleanupExcessTokensAsync(Guid userId, string? deviceId)
	{
		// 如果指定了设备ID，先撤销该设备的旧Token
		if (!string.IsNullOrEmpty(deviceId))
		{
			await _refreshTokenRepository.RevokeByUserIdAndDeviceAsync(userId, deviceId);
		}

		// 检查活跃Token数量
		var activeCount = await _refreshTokenRepository.GetActiveTokenCountByUserIdAsync(userId);

		if (activeCount >= AuthenticationConsts.MaxActiveTokensPerUser)
		{
			// 获取所有活跃Token，按创建时间排序
			var activeTokens = await _refreshTokenRepository.GetActiveTokensByUserIdAsync(userId);

			// 撤销最旧的Token，保留最新的 MaxActiveTokensPerUser - 1 个
			var tokensToRevoke = activeTokens
				.OrderByDescending(t => t.CreationTime)
				.Skip(AuthenticationConsts.MaxActiveTokensPerUser - 1)
				.ToList();

			foreach (var token in tokensToRevoke)
			{
				token.Revoke(_clock.Now);
				await _refreshTokenRepository.UpdateAsync(token);
			}
		}
	}

	private static string? GetClientIpAddress(HttpContext? httpContext)
	{
		if (httpContext == null) return null;

		// 优先从代理头获取
		var forwardedFor = httpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrEmpty(forwardedFor))
		{
			return forwardedFor.Split(',')[0].Trim();
		}

		var realIp = httpContext.Request.Headers["X-Real-IP"].FirstOrDefault();
		if (!string.IsNullOrEmpty(realIp))
		{
			return realIp;
		}

		return httpContext.Connection.RemoteIpAddress?.ToString();
	}

	private string? GetCurrentRefreshTokenFromHeader()
	{
		// 可以通过自定义Header传递当前RefreshToken用于标识当前会话
		return _httpContextAccessor.HttpContext?.Request.Headers["X-Refresh-Token"].FirstOrDefault();
	}

	#endregion
}
