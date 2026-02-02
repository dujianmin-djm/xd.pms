using Microsoft.Extensions.Localization;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using XD.Pms.ApiResponse;
using XD.Pms.Localization;

namespace XD.Pms.Authentication;

public class TokenAnalysisService : ITokenAnalysisService, ITransientDependency
{
	private readonly ITokenBlacklistService _blacklistService;
	private readonly IStringLocalizer<PmsResource> _localizer;
	private readonly JwtSecurityTokenHandler _tokenHandler;
	private readonly IClock _clock;

	public TokenAnalysisService(ITokenBlacklistService blacklistService,IStringLocalizer<PmsResource> localizer, IClock clock)
	{
		_blacklistService = blacklistService;
		_localizer = localizer;
		_clock = clock;
		_tokenHandler = new JwtSecurityTokenHandler();
	}

	public async Task<TokenAnalysisResult> AnalyzeTokenErrorAsync(string? token, string? wwwAuthenticate = null)
	{
		// 1. 检查是否提供了 Token
		if (string.IsNullOrWhiteSpace(token))
		{
			return new TokenAnalysisResult
			{
				Code = ApiResponseCode.Unauthorized,
				Message = _localizer["Auth:AccessTokenMissing"].Value,
				Status = TokenStatus.Missing,
				Details = "No token provided in request"
			};
		}

		// 2. 尝试解析 Token
		JwtSecurityToken? jwtToken = null;
		try
		{
			if (!_tokenHandler.CanReadToken(token))
			{
				return new TokenAnalysisResult
				{
					Code = ApiResponseCode.AccessTokenInvalid,
					Message = _localizer["Auth:AccessTokenInvalid"].Value,
					Status = TokenStatus.Invalid,
					Details = "Token format is invalid, cannot be read as JWT"
				};
			}
			jwtToken = _tokenHandler.ReadJwtToken(token);
		}
		catch (Exception ex)
		{
			return new TokenAnalysisResult
			{
				Code = ApiResponseCode.AccessTokenInvalid,
				Message = _localizer["Auth:AccessTokenInvalid"].Value,
				Status = TokenStatus.Invalid,
				Details = $"Failed to parse token: {ex.Message}"
			};
		}

		// 3. 检查 Token 是否过期
		if (jwtToken.ValidTo != DateTime.MinValue && jwtToken.ValidTo < DateTime.UtcNow)
		{
			var expiredAt = jwtToken.ValidTo;
			var expiredDuration = DateTime.UtcNow - expiredAt;
			var expiredAtLocal = _clock.Normalize(expiredAt);
			return new TokenAnalysisResult
			{
				Code = ApiResponseCode.AccessTokenExpired,
				Message = _localizer["Auth:AccessTokenExpired"].Value,
				Status = TokenStatus.Expired,
				Details = $"Token expired at {expiredAtLocal:yyyy-MM-dd HH:mm:ss} (Local), expired for {expiredDuration.TotalMinutes:F1} minutes"
			};
		}

		// 4. 检查 Token 是否在黑名单中（已被撤销）
		var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
		if (!string.IsNullOrEmpty(jti))
		{
			if (await _blacklistService.IsBlacklistedAsync(jti))
			{
				return new TokenAnalysisResult
				{
					Code = ApiResponseCode.AccessTokenRevoked,
					Message = _localizer["Auth:AccessTokenRevoked"].Value,
					Status = TokenStatus.Revoked,
					Details = $"Token (jti: {jti}) has been revoked"
				};
			}
		}

		// 5. 分析 WWW-Authenticate header
		if (!string.IsNullOrEmpty(wwwAuthenticate))
		{
			var headerAnalysis = AnalyzeWwwAuthenticate(wwwAuthenticate);
			if (headerAnalysis != null)
			{
				return headerAnalysis;
			}
		}

		// 6. Token 看起来有效但认证仍然失败，可能是签名问题
		return new TokenAnalysisResult
		{
			Code = ApiResponseCode.AccessTokenInvalid,
			Message = _localizer["Auth:AccessTokenInvalid"].Value,
			Status = TokenStatus.SignatureInvalid,
			Details = "Token appears valid but authentication failed, possibly signature verification failed"
		};
	}

	private TokenAnalysisResult? AnalyzeWwwAuthenticate(string wwwAuthenticate)
	{
		// 解析 WWW-Authenticate header
		// 格式: Bearer error="invalid_token", error_description="The specified token is no longer valid."

		var errorMatch = Regex.Match(wwwAuthenticate, @"error=""([^""]+)""");
		var descriptionMatch = Regex.Match(wwwAuthenticate, @"error_description=""([^""]+)""");

		var error = errorMatch.Success ? errorMatch.Groups[1].Value : null;
		var description = descriptionMatch.Success ? descriptionMatch.Groups[1].Value : null;

		if (string.IsNullOrEmpty(error))
		{
			return null;
		}

		// 根据 OpenIddict 的错误信息判断
		return (error.ToLowerInvariant(), description?.ToLowerInvariant()) switch
		{
			("invalid_token", var desc) when desc?.Contains("expired") == true
				=> CreateResult(ApiResponseCode.AccessTokenExpired, "Auth:AccessTokenExpired", TokenStatus.Expired, description),

			("invalid_token", "the specified token is no longer valid.")
				=> null,

			("invalid_token", var desc) when desc?.Contains("revoked") == true
				=> CreateResult(ApiResponseCode.AccessTokenRevoked, "Auth:AccessTokenRevoked", TokenStatus.Revoked, description),

			("invalid_token", _)
				=> CreateResult(ApiResponseCode.AccessTokenInvalid, "Auth:AccessTokenInvalid", TokenStatus.Invalid, description),

			_ => null
		};
	}

	private TokenAnalysisResult CreateResult(string code, string messageKey, TokenStatus status, string? details)
	{
		return new TokenAnalysisResult
		{
			Code = code,
			Message = _localizer[messageKey].Value,
			Status = status,
			Details = details
		};
	}
}