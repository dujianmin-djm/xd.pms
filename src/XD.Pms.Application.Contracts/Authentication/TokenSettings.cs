namespace XD.Pms.Authentication;

/// <summary>
/// Token 配置
/// </summary>
public class TokenSettings
{
	public const string SectionName = "AuthServer:TokenSettings";

	/// <summary>
	/// Access Token 有效期（分钟），默认 30 分钟
	/// </summary>
	public int AccessTokenExpirationMinutes { get; set; } = 30;

	/// <summary>
	/// Refresh Token 有效期（天），默认 7 天
	/// </summary>
	public int RefreshTokenExpirationDays { get; set; } = 7;
}