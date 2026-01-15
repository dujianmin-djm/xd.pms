using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Identity;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Authentication;

/// <summary>
/// Token 认证服务（封装 OpenIddict）
/// </summary>
[RemoteService(IsEnabled = false)]
public class TokenAppService : PmsAppService, ITokenAppService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IdentityUserManager _userManager;
	private readonly IConfiguration _configuration;
	private readonly TokenSettings _tokenSettings;

	public TokenAppService(
		IHttpClientFactory httpClientFactory,
		IHttpContextAccessor httpContextAccessor,
		IdentityUserManager userManager,
		IConfiguration configuration,
		IOptions<TokenSettings> tokenSettings)
	{
		_httpClientFactory = httpClientFactory;
		_httpContextAccessor = httpContextAccessor;
		_userManager = userManager;
		_configuration = configuration;
		_tokenSettings = tokenSettings.Value;
	}

	public async Task<LoginResponseDto> LoginAsync(LoginRequestDto input)
	{
		var clientId = input.ClientId ?? GetDefaultClientId();
		var scope = input.Scope ?? "openid profile email phone roles Pms offline_access";

		var tokenEndpoint = GetTokenEndpoint();

		var requestData = new Dictionary<string, string>
		{
			["grant_type"] = "password",
			["client_id"] = clientId,
			["username"] = input.UserNameOrEmail,
			["password"] = input.Password,
			["scope"] = scope
		};

		// 调用 OpenIddict Token 端点
		var tokenResponse = await RequestTokenAsync(tokenEndpoint, requestData, "login");

		// 获取用户信息
		var user = await _userManager.FindByNameAsync(input.UserNameOrEmail)
				   ?? await _userManager.FindByEmailAsync(input.UserNameOrEmail)
				   ?? throw new UserFriendlyException("当前账号不存在");

		if (!user.IsActive)
		{
			throw new UserFriendlyException("账户尚未启用");
		}

		if (await _userManager.IsLockedOutAsync(user))
		{
			throw new UserFriendlyException("账户已被锁定，请稍后再试");
		}

		var roles = await _userManager.GetRolesAsync(user);

		return new LoginResponseDto
		{
			AccessToken = tokenResponse.AccessToken,
			RefreshToken = tokenResponse.RefreshToken,
			TokenType = tokenResponse.TokenType ?? "Bearer",
			ExpiresIn = tokenResponse.ExpiresIn,
			AccessTokenExpiration = Clock.Now.AddSeconds(tokenResponse.ExpiresIn),
			RefreshTokenExpiration = Clock.Now.AddDays(_tokenSettings.RefreshTokenExpirationDays),
			Scope = tokenResponse.Scope,
			User = new UserInfoDto
			{
				Id = user.Id,
				UserName = user.UserName!,
				Email = user.Email,
				EmailConfirmed = user.EmailConfirmed,
				Name = user.Name,
				Surname = user.Surname,
				PhoneNumber = user.PhoneNumber,
				PhoneNumberConfirmed = user.PhoneNumberConfirmed,
				TwoFactorEnabled = user.TwoFactorEnabled,
				Roles = [.. roles],
				TenantId = user.TenantId
			}
		};
	}

	public async Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input)
	{
		var clientId = input.ClientId ?? GetDefaultClientId();
		var tokenEndpoint = GetTokenEndpoint();

		var requestData = new Dictionary<string, string>
		{
			["grant_type"] = "refresh_token",
			["client_id"] = clientId,
			["refresh_token"] = input.RefreshToken
		};

		var tokenResponse = await RequestTokenAsync(tokenEndpoint, requestData, "refresh");

		return new TokenResponseDto
		{
			AccessToken = tokenResponse.AccessToken,
			RefreshToken = tokenResponse.RefreshToken,
			TokenType = tokenResponse.TokenType ?? "Bearer",
			ExpiresIn = tokenResponse.ExpiresIn,
			AccessTokenExpiration = Clock.Now.AddSeconds(tokenResponse.ExpiresIn),
			RefreshTokenExpiration = Clock.Now.AddDays(_tokenSettings.RefreshTokenExpirationDays),
			Scope = tokenResponse.Scope
		};
	}

	[Authorize]
	public async Task RevokeTokenAsync(string? token = null)
	{
		var accessToken = token ?? GetCurrentAccessToken();

		if (string.IsNullOrEmpty(accessToken))
		{
			return;
		}

		var revocationEndpoint = GetRevocationEndpoint();
		var clientId = GetDefaultClientId();

		var client = _httpClientFactory.CreateClient();
		var content = new FormUrlEncodedContent(new Dictionary<string, string>
		{
			["client_id"] = clientId,
			["token"] = accessToken,
			["token_type_hint"] = "access_token"
		});

		try
		{
			await client.PostAsync(revocationEndpoint, content);
		}
		catch
		{
			// 忽略撤销失败（Token 可能已过期）
		}
	}

	[Authorize]
	public async Task<UserInfoDto> GetCurrentUserInfoAsync()
	{
		if (CurrentUser.Id == null)
		{
			throw new UserFriendlyException("用户未登录");
		}

		var user = await _userManager.FindByIdAsync(CurrentUser.Id.Value.ToString()) 
				?? throw new UserFriendlyException("用户不存在");

		var roles = await _userManager.GetRolesAsync(user);

		return new UserInfoDto
		{
			Id = user.Id,
			UserName = user.UserName!,
			Email = user.Email,
			EmailConfirmed = user.EmailConfirmed,
			Name = user.Name,
			Surname = user.Surname,
			PhoneNumber = user.PhoneNumber,
			PhoneNumberConfirmed = user.PhoneNumberConfirmed,
			TwoFactorEnabled = user.TwoFactorEnabled,
			Roles = [.. roles],
			TenantId = user.TenantId
		};
	}

	#region 私有方法

	private async Task<OpenIddictTokenResponse> RequestTokenAsync(string endpoint, Dictionary<string, string> data, string operation)
	{
		var client = _httpClientFactory.CreateClient();
		var content = new FormUrlEncodedContent(data);

		var response = await client.PostAsync(endpoint, content);
		var responseContent = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode)
		{
			var error = JsonSerializer.Deserialize<OpenIddictErrorResponse>(responseContent);
			var (code, message) = GetFriendlyError(error?.Error, error?.ErrorDescription, operation);

			throw new AuthenticationException(code, message);
		}

		var tokenResponse = JsonSerializer.Deserialize<OpenIddictTokenResponse>(responseContent);

		if (tokenResponse == null || string.IsNullOrEmpty(tokenResponse.AccessToken))
		{
			throw new AuthenticationException(ApiResponseCode.InternalError, "获取令牌失败");
		}

		return tokenResponse;
	}

	private string GetTokenEndpoint()
	{
		var request = _httpContextAccessor.HttpContext?.Request 
				?? throw new UserFriendlyException("无法获取请求上下文");

		var authority = _configuration["AuthServer:Authority"]?.TrimEnd('/')
						?? $"{request.Scheme}://{request.Host}";

		return $"{authority}/connect/token";
	}

	private string GetRevocationEndpoint()
	{
		var request = _httpContextAccessor.HttpContext?.Request
					?? throw new UserFriendlyException("无法获取请求上下文");
		var authority = _configuration["AuthServer:Authority"]?.TrimEnd('/')
						?? $"{request.Scheme}://{request.Host}";

		return $"{authority}/connect/revocation";
	}

	private string GetDefaultClientId()
	{
		return _configuration["OpenIddict:Applications:Pms_App:ClientId"] ?? "Pms_App";
	}

	private string? GetCurrentAccessToken()
	{
		var authHeader = _httpContextAccessor.HttpContext?.Request.Headers["Authorization"].ToString();

		if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
		{
			return null;
		}

		return authHeader.Substring("Bearer ".Length).Trim();
	}

	/// <summary>
	/// 获取友好的错误信息
	/// </summary>
	private static (string Code, string Message) GetFriendlyError(string? error, string? description, string operation)
	{
		// 根据操作类型和错误类型返回不同的提示
		return (error, operation) switch
		{
			// ==================== 登录场景 ====================

			// 账户被锁定
			("invalid_grant", "login") when ContainsAny(description, "locked", "lockout")
				=> (ApiResponseCode.AccountLocked, "账户已被锁定，请稍后再试或联系管理员"),

			// 账户被禁用
			("invalid_grant", "login") when ContainsAny(description, "disabled", "inactive")
				=> (ApiResponseCode.AccountDisabled, "账户已被禁用，请联系管理员"),

			// 账户不允许登录
			("invalid_grant", "login") when ContainsAny(description, "not allowed", "denied")
				=> (ApiResponseCode.Forbidden, "该账户不允许登录"),

			// 用户不存在
			("invalid_grant", "login") when ContainsAny(description, "user not found", "no user")
				=> (ApiResponseCode.InvalidCredentials, "用户名或密码错误"),

			// 密码错误 / 默认登录错误
			("invalid_grant", "login")
				=> (ApiResponseCode.InvalidCredentials, "用户名或密码错误"),

			// ==================== 刷新 Token 场景 ====================

			// Token 已过期
			("invalid_grant", "refresh") when ContainsAny(description, "expired")
				=> (ApiResponseCode.RefreshTokenExpired, "登录已过期，请重新登录"),

			// Token 已被撤销
			("invalid_grant", "refresh") when ContainsAny(description, "revoked", "invalidated")
				=> (ApiResponseCode.TokenInvalid, "登录凭证已失效，请重新登录"),

			// Token 无效 / 默认刷新错误
			("invalid_grant", "refresh")
				=> (ApiResponseCode.RefreshTokenExpired, "刷新令牌无效，请重新登录"),

			// ==================== 通用错误 ====================

			// 客户端配置错误
			("invalid_client", _)
				=> (ApiResponseCode.InternalError, "客户端配置错误，请联系管理员"),

			// 权限范围无效
			("invalid_scope", _)
				=> (ApiResponseCode.BadRequest, "请求的权限范围无效"),

			// 客户端未授权
			("unauthorized_client", _)
				=> (ApiResponseCode.Forbidden, "客户端未授权使用此认证方式"),

			// 请求参数错误
			("invalid_request", _)
				=> (ApiResponseCode.ValidationError, description ?? "请求参数错误"),

			// 访问被拒绝
			("access_denied", _)
				=> (ApiResponseCode.Forbidden, "访问被拒绝"),

			// 需要二次验证
			("mfa_required", _)
				=> (ApiResponseCode.TwoFactorRequired, "需要进行二次验证"),

			// 二次验证失败
			("invalid_mfa", _)
				=> (ApiResponseCode.TwoFactorInvalid, "验证码错误，请重试"),

			// 默认错误
			_ => (ApiResponseCode.InternalError, description ?? "认证失败，请稍后重试")
		};
	}

	/// <summary>
	/// 检查字符串是否包含任意关键词（忽略大小写）
	/// </summary>
	private static bool ContainsAny(string? text, params string[] keywords)
	{
		if (string.IsNullOrEmpty(text)) return false;
		return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
	}

	#endregion

	#region 辅助类

	private class OpenIddictTokenResponse
	{
		[JsonPropertyName("access_token")]
		public string AccessToken { get; set; } = default!;

		[JsonPropertyName("refresh_token")]
		public string RefreshToken { get; set; } = default!;

		[JsonPropertyName("token_type")]
		public string? TokenType { get; set; }

		[JsonPropertyName("expires_in")]
		public int ExpiresIn { get; set; }

		[JsonPropertyName("scope")]
		public string? Scope { get; set; }

		[JsonPropertyName("id_token")]
		public string? IdToken { get; set; }
	}

	private class OpenIddictErrorResponse
	{
		[JsonPropertyName("error")]
		public string? Error { get; set; }

		[JsonPropertyName("error_description")]
		public string? ErrorDescription { get; set; }

		[JsonPropertyName("error_uri")]
		public string? ErrorUri { get; set; }
	}

	#endregion
}