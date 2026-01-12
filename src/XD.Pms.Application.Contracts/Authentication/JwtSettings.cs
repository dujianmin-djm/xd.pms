namespace XD.Pms.Authentication;

/// <summary>
/// JWT 配置选项
/// </summary>
public class JwtSettings
{
	public const string SectionName = "Jwt";

	/// <summary>
	/// 密钥
	/// </summary>
	public string SecretKey { get; set; } = default!;

	/// <summary>
	/// 发行者
	/// </summary>
	public string Issuer { get; set; } = default!;

	/// <summary>
	/// 受众
	/// </summary>
	public string Audience { get; set; } = default!;

	/// <summary>
	/// Access Token 过期时间（分钟）
	/// </summary>
	public int AccessTokenExpirationMinutes { get; set; } = 30;

	/// <summary>
	/// Refresh Token 过期时间（天）
	/// </summary>
	public int RefreshTokenExpirationDays { get; set; } = 7;

	/// <summary>
	/// 记住我时 Refresh Token 过期时间（天）
	/// </summary>
	public int RememberMeRefreshTokenExpirationDays { get; set; } = 30;
}
