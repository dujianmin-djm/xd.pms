using System;
using System.Collections.Generic;
using System.Security.Claims;

namespace XD.Pms.Authentication;

public interface IJwtTokenGenerator
{
	/// <summary>
	/// 生成 Access Token
	/// </summary>
	(string Token, DateTime Expiration) GenerateAccessToken(
		Guid userId,
		string userName,
		string? email,
		IEnumerable<string> roles,
		Guid? tenantId = null,
		IDictionary<string, string>? additionalClaims = null);

	/// <summary>
	/// 生成 Refresh Token
	/// </summary>
	string GenerateRefreshToken();

	/// <summary>
	/// 验证 Access Token（不验证过期）
	/// </summary>
	ClaimsPrincipal? ValidateAccessToken(string token, bool validateLifetime = true);
}
