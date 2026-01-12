namespace XD.Pms.Authentication;

public static class AuthenticationConsts
{
	/// <summary>
	/// Access Token 默认过期时间（分钟）
	/// </summary>
	//public const int DefaultAccessTokenExpirationMinutes = 10;

	/// <summary>
	/// Refresh Token 默认过期时间（天）
	/// </summary>
	//public const int DefaultRefreshTokenExpirationDays = 7;

	/// <summary>
	/// Refresh Token 最大长度
	/// </summary>
	public const int RefreshTokenMaxLength = 256;

	/// <summary>
	/// Refresh Token 保留时间（天）
	/// </summary>
	public const int RefreshTokenRetentionDays = 30;

	/// <summary>
	/// 设备标识最大长度
	/// </summary>
	public const int DeviceIdMaxLength = 128;

	/// <summary>
	/// 客户端信息最大长度
	/// </summary>
	public const int ClientInfoMaxLength = 512;

	/// <summary>
	/// 单用户最大活跃Token数量
	/// </summary>
	public const int MaxActiveTokensPerUser = 5;
}
