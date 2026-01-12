using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;
using Volo.Abp.Timing;

namespace XD.Pms.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator, ITransientDependency
{
	private readonly JwtSettings _jwtSettings;
	private readonly IClock _clock;

	public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings, IClock clock)
	{
		_jwtSettings = jwtSettings.Value;
		_clock = clock;
	}

	public (string Token, DateTime Expiration) GenerateAccessToken(
		Guid userId,
		string userName,
		string? email,
		IEnumerable<string> roles,
		Guid? tenantId = null,
		IDictionary<string, string>? additionalClaims = null)
	{
		var expiration = _clock.Now.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes);

		var claims = new List<Claim>
		{
			new(AbpClaimTypes.UserId, userId.ToString()),
			new(AbpClaimTypes.UserName, userName),
			new(JwtRegisteredClaimNames.Sub, userId.ToString()),
			new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
			new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
				ClaimValueTypes.Integer64)
		};

		if (!string.IsNullOrEmpty(email))
		{
			claims.Add(new Claim(AbpClaimTypes.Email, email));
		}

		if (tenantId.HasValue)
		{
			claims.Add(new Claim(AbpClaimTypes.TenantId, tenantId.Value.ToString()));
		}

		// 添加角色
		foreach (var role in roles)
		{
			claims.Add(new Claim(AbpClaimTypes.Role, role));
		}

		// 添加额外声明
		if (additionalClaims != null)
		{
			foreach (var claim in additionalClaims)
			{
				claims.Add(new Claim(claim.Key, claim.Value));
			}
		}

		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var token = new JwtSecurityToken(
			issuer: _jwtSettings.Issuer,
			audience: _jwtSettings.Audience,
			claims: claims,
			expires: expiration,
			signingCredentials: credentials
		);

		return (new JwtSecurityTokenHandler().WriteToken(token), expiration);
	}

	public string GenerateRefreshToken()
	{
		var randomBytes = new byte[64];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(randomBytes);
		return Convert.ToBase64String(randomBytes);
	}

	public ClaimsPrincipal? ValidateAccessToken(string token, bool validateLifetime = true)
	{
		try
		{
			var tokenHandler = new JwtSecurityTokenHandler();
			var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

			var validationParameters = new TokenValidationParameters
			{
				ValidateIssuerSigningKey = true,
				IssuerSigningKey = new SymmetricSecurityKey(key),
				ValidateIssuer = true,
				ValidIssuer = _jwtSettings.Issuer,
				ValidateAudience = true,
				ValidAudience = _jwtSettings.Audience,
				ValidateLifetime = validateLifetime,
				ClockSkew = TimeSpan.Zero
			};

			var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
			return principal;
		}
		catch
		{
			return null;
		}
	}
}
