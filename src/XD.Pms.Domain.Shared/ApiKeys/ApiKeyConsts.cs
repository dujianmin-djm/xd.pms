namespace XD.Pms.ApiKeys;

public static class ApiKeyConsts
{
	/// <summary>
	/// 默认请求头名称
	/// </summary>
	public const string DefaultHeaderName = "X-API-Sign";

	/// <summary>
	/// Key 前缀最大长度
	/// </summary>
	public const int MaxKeyPrefixLength = 20;

	/// <summary>
	/// 客户端 ID 最大长度
	/// </summary>
	public const int MaxClientIdLength = 64;

	/// <summary>
	/// 客户端名称最大长度
	/// </summary>
	public const int MaxClientNameLength = 128;

	/// <summary>
	/// 描述最大长度
	/// </summary>
	public const int MaxDescriptionLength = 500;

	/// <summary>
	/// Key 哈希长度
	/// </summary>
	public const int KeyHashLength = 64;

	/// <summary>
	/// 角色字段最大长度
	/// </summary>
	public const int MaxRolesLength = 1000;

	/// <summary>
	/// 权限字段最大长度
	/// </summary>
	public const int MaxPermissionsLength = 2000;

	/// <summary>
	/// IP 地址字段最大长度
	/// </summary>
	public const int MaxAllowedIpAddressesLength = 1000;

	/// <summary>
	/// 缓存前缀
	/// </summary>
	public const string CacheKeyPrefix = "ApiKey:";

	/// <summary>
	/// 缓存过期时间（分钟）
	/// </summary>
	public const int CacheExpirationMinutes = 30;

	/// <summary>
	/// 无效 Key 缓存过期时间（分钟）- 防止暴力破解
	/// </summary>
	public const int InvalidKeyCacheExpirationMinutes = 5;
}