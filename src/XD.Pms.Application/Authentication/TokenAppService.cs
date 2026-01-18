using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Authentication;

/// <summary>
/// Token 认证服务（封装 OpenIddict）
/// </summary>
public class TokenAppService : PmsAppService, ITokenAppService
{
	private readonly IHttpClientFactory _httpClientFactory;
	private readonly IHttpContextAccessor _httpContextAccessor;
	private readonly IdentityUserManager _userManager;
	private readonly IConfiguration _configuration;
	private readonly TokenSettings _tokenSettings;
	private readonly ITokenBlacklistService _tokenBlacklistService;

	public TokenAppService(
		IHttpClientFactory httpClientFactory,
		IHttpContextAccessor httpContextAccessor,
		IdentityUserManager userManager,
		IConfiguration configuration,
		IOptions<TokenSettings> tokenSettings,
		ITokenBlacklistService tokenBlacklistService)
	{
		_httpClientFactory = httpClientFactory;
		_httpContextAccessor = httpContextAccessor;
		_userManager = userManager;
		_configuration = configuration;
		_tokenSettings = tokenSettings.Value;
		_tokenBlacklistService = tokenBlacklistService;
	}

	/// <summary>
	/// 用户登录
	/// </summary>
	public async Task<LoginResponseDto> LoginAsync(LoginRequestDto input)
	{
		// 构建请求参数
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
				   ?? throw new AuthenticationException(ApiResponseCode.InvalidCredentials, L["Auth:AccountNotFound"].Value);
		
		if (!user.IsActive)
		{
			throw new AuthenticationException(ApiResponseCode.InvalidCredentials, L["Auth:AccountDisabled"].Value);
		}

		if (await _userManager.IsLockedOutAsync(user))
		{
			throw new AuthenticationException(ApiResponseCode.InvalidCredentials, L["Auth:AccountLocked"].Value);
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
			Language = CultureInfo.CurrentCulture.Name,
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

	/// <summary>
	/// 刷新令牌
	/// </summary>
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
			Scope = tokenResponse.Scope,
			Language = CultureInfo.CurrentCulture.Name
		};
	}

	/// <summary>
	/// 撤销令牌
	/// </summary>
	[Authorize]
	public async Task RevokeTokenAsync(string? token = null)
	{
		var accessToken = token ?? GetCurrentAccessToken();

		if (string.IsNullOrEmpty(accessToken))
		{
			return;
		}

		// 解析 Token 获取 JTI 和过期时间
		var (jti, expiration) = ParseTokenInfo(accessToken);

		if (!string.IsNullOrEmpty(jti) && expiration.HasValue)
		{
			// 加入黑名单
			await _tokenBlacklistService.AddToBlacklistAsync(jti, expiration.Value);
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

	/// <summary>
	/// 获取当前用户信息
	/// </summary>
	[Authorize]
	public async Task<UserInfoDto> GetCurrentUserInfoAsync()
	{
		if (CurrentUser.Id == null)
		{
			throw new AuthenticationException(ApiResponseCode.Unauthorized, L["Auth:Unauthorized"].Value);
		}

		var user = await _userManager.FindByIdAsync(CurrentUser.Id.Value.ToString())
			?? throw new AuthenticationException(ApiResponseCode.AccountNotFound, L["Auth:AccountNotFound"].Value);

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
			throw new AuthenticationException(ApiResponseCode.InternalError, L["Auth:GetTokenFailed"].Value);
		}

		return tokenResponse;
	}

	private string GetTokenEndpoint()
	{
		var request = _httpContextAccessor.HttpContext.Request;

		var authority = _configuration["AuthServer:Authority"]?.TrimEnd('/')
						?? $"{request.Scheme}://{request.Host}";

		return $"{authority}/connect/token";
	}

	private string GetRevocationEndpoint()
	{
		var request = _httpContextAccessor.HttpContext.Request;

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
	/// 获取友好的错误信息（多语言）
	/// </summary>
	private (string Code, string Message) GetFriendlyError(string? error, string? description, string operation)
	{
		return (error, operation) switch
		{
			// ==================== 登录场景 ====================
			("invalid_grant", "login") when ContainsAny(description, "locked", "lockout")
				=> (ApiResponseCode.AccountLocked, L["Auth:AccountLocked"].Value),

			("account_inactive", "login")
				=> (ApiResponseCode.AccountDisabled, L["Auth:AccountDisabled"].Value),

			("invalid_grant", "login") when ContainsAny(description, "not allowed", "denied")
				=> (ApiResponseCode.Forbidden, L["Auth:AccountNotAllowed"].Value),

			("invalid_grant", "login") when ContainsAny(description, "user not found", "no user")
				=> (ApiResponseCode.InvalidCredentials, L["Auth:InvalidCredentials"].Value),

			("invalid_grant", "login")
				=> (ApiResponseCode.InvalidCredentials, L["Auth:InvalidCredentials"].Value),

			// ==================== 刷新 Token 场景 ====================
			("invalid_grant", "refresh") when ContainsAny(description, "expired")
				=> (ApiResponseCode.RefreshTokenExpired, L["Auth:RefreshTokenExpired"].Value),

			("invalid_grant", "refresh") when ContainsAny(description, "revoked", "invalidated")
				=> (ApiResponseCode.TokenInvalid, L["Auth:TokenRevoked"].Value),

			("invalid_grant", "refresh")
				=> (ApiResponseCode.RefreshTokenExpired, L["Auth:RefreshTokenInvalid"].Value),

			// ==================== 通用错误 ====================
			("invalid_client", _)
				=> (ApiResponseCode.InternalError, L["Auth:ClientConfigError"].Value),

			("invalid_scope", _)
				=> (ApiResponseCode.BadRequest, L["Auth:InvalidScope"].Value),

			("unauthorized_client", _)
				=> (ApiResponseCode.Forbidden, L["Auth:ClientUnauthorized"].Value),

			("invalid_request", _)
				=> (ApiResponseCode.ValidationError, description ?? L["Auth:InvalidRequest"].Value),

			("access_denied", _)
				=> (ApiResponseCode.Forbidden, L["Auth:AccessDenied"].Value),

			("mfa_required", _)
				=> (ApiResponseCode.TwoFactorRequired, L["Auth:TwoFactorRequired"].Value),

			("invalid_mfa", _)
				=> (ApiResponseCode.TwoFactorInvalid, L["Auth:TwoFactorInvalid"].Value),

			// 默认错误
			_ => (ApiResponseCode.InternalError, description ?? L["Auth:AuthenticationFailed"].Value)
		};
	}

	private static bool ContainsAny(string? text, params string[] keywords)
	{
		if (string.IsNullOrEmpty(text)) return false;
		return keywords.Any(k => text.Contains(k, StringComparison.OrdinalIgnoreCase));
	}

	private static (string? Jti, DateTime? Expiration) ParseTokenInfo(string token)
	{
		try
		{
			var handler = new JwtSecurityTokenHandler();

			if (!handler.CanReadToken(token))
			{
				return (null, null);
			}

			var jwtToken = handler.ReadJwtToken(token);
			var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
			var exp = jwtToken.ValidTo;

			return (jti, exp);
		}
		catch
		{
			return (null, null);
		}
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