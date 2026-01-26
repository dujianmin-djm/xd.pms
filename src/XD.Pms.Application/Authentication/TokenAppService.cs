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
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using Volo.Abp.Json;
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
	private readonly IJsonSerializer _jsonSerializer;

	public TokenAppService(
		IHttpClientFactory httpClientFactory,
		IHttpContextAccessor httpContextAccessor,
		IdentityUserManager userManager,
		IConfiguration configuration,
		IOptions<TokenSettings> tokenSettings,
		ITokenBlacklistService tokenBlacklistService,
		IJsonSerializer jsonSerializer)
	{
		_httpClientFactory = httpClientFactory;
		_httpContextAccessor = httpContextAccessor;
		_userManager = userManager;
		_configuration = configuration;
		_tokenSettings = tokenSettings.Value;
		_tokenBlacklistService = tokenBlacklistService;
		_jsonSerializer = jsonSerializer;
	}

	/// <summary>
	/// 用户登录
	/// </summary>
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

		return await GetRequestTokenResponseAsync(tokenEndpoint, requestData, "login");
	}

	/// <summary>
	/// 刷新令牌
	/// </summary>
	public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input)
	{
		var clientId = input.ClientId ?? GetDefaultClientId();
		var tokenEndpoint = GetTokenEndpoint();

		var requestData = new Dictionary<string, string>
		{
			["grant_type"] = "refresh_token",
			["client_id"] = clientId,
			["refresh_token"] = input.RefreshToken
		};

		return await GetRequestTokenResponseAsync(tokenEndpoint, requestData, "refresh");
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
			// 忽略撤销失败
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
			?? throw new AuthenticationException(ApiResponseCode.BadRequest, L["Auth:AccountNotFound"].Value);

		var roles = await _userManager.GetRolesAsync(user);

		return new UserInfoDto
		{
			Id = user.Id,
			UserName = user.UserName!,
			Email = user.Email,
			PhoneNumber = user.PhoneNumber,
			Roles = [.. roles],
			Buttons = []
		};
	}


	private async Task<LoginResponseDto> GetRequestTokenResponseAsync(string endpoint, Dictionary<string, string> data, string operation)
	{
		var client = _httpClientFactory.CreateClient();
		var content = new FormUrlEncodedContent(data);

		var response = await client.PostAsync(endpoint, content);
		var responseContent = await response.Content.ReadAsStringAsync();

		if (!response.IsSuccessStatusCode)
		{
			var error = _jsonSerializer.Deserialize<OpenIddictErrorResponse>(responseContent);
			var (code, message) = GetFriendlyTokenError(error?.Error, error?.ErrorDescription, operation);
			string details = "Request Content:" + _jsonSerializer.Serialize(data, indented: true) + "\r\nResponse:" + responseContent;
			throw new AuthenticationException(code, message, details);
		}

		var tokenResponse = _jsonSerializer.Deserialize<OpenIddictTokenResponse>(responseContent);

		if (string.IsNullOrEmpty(tokenResponse?.AccessToken))
		{
			throw new AuthenticationException(ApiResponseCode.InternalError, L["Auth:GetTokenFailed"].Value);
		}

		return new LoginResponseDto
		{
			AccessToken = tokenResponse.AccessToken,
			RefreshToken = tokenResponse.RefreshToken,
			TokenType = tokenResponse.TokenType ?? "Bearer",
			ExpiresIn = tokenResponse.ExpiresIn,
			AccessTokenExpiration = Clock.Now.AddSeconds(tokenResponse.ExpiresIn),
			RefreshTokenExpiration = Clock.Now.AddDays(_tokenSettings.RefreshTokenExpirationDays),
			Language = CultureInfo.CurrentCulture.Name
		};
	}

	private string GetTokenEndpoint()
	{
		var request = _httpContextAccessor.HttpContext.Request;
		var authority = $"{request.Scheme}://{request.Host}";
		return $"{authority}/connect/token";
	}

	private string GetRevocationEndpoint()
	{
		var request = _httpContextAccessor.HttpContext.Request;
		var authority = $"{request.Scheme}://{request.Host}";
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

	private (string Code, string Message) GetFriendlyTokenError(string? error, string? description, string operation)
	{
		return (error, operation, description) switch
		{
			// ==================== 登录场景 ====================
			("account_locked", "login", _)
				=> (ApiResponseCode.AccountLocked, L["Auth:AccountLocked"].Value),

			("account_inactive", "login", _)
				=> (ApiResponseCode.AccountDisabled, L["Auth:AccountDisabled"].Value),

			("invalid_grant", "login", _)
				=> (ApiResponseCode.InvalidCredentials, L["Auth:InvalidCredentials"].Value),

			// ==================== 刷新 Token 场景 ====================
			("invalid_grant", "refresh", "The specified refresh token is no longer valid.")
				=> (ApiResponseCode.RefreshTokenExpired, L["Auth:RefreshTokenExpired"].Value),

			("invalid_grant", "refresh", "The specified refresh token has already been redeemed.")
				=> (ApiResponseCode.RefreshTokenRedeemed, L["Auth:RefreshTokenRedeemed"].Value),

			("invalid_grant", "refresh", _)
				=> (ApiResponseCode.SessionExpired, L["Auth:RefreshTokenInvalid"].Value),

			// ==================== 其他错误 ====================
			("invalid_client", _, _)
				=> (ApiResponseCode.BadRequest, description ?? L["Auth:ClientConfigError"].Value),

			("invalid_scope", _, _) => (ApiResponseCode.BadRequest, description ?? L["Auth:InvalidScope"].Value),

			// 默认错误
			_ => (ApiResponseCode.Forbidden, $"{error}: {description}" ?? L["Auth:AuthenticationFailed"].Value)
		};
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