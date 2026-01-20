namespace XD.Pms.ApiResponse;

/// <summary>
/// API 响应状态码
/// </summary>
public static class ApiResponseCode
{
	#region 成功 (200)

	/// <summary>
	/// 成功
	/// </summary>
	public const string Success = "200";

	#endregion

	#region 请求错误 (400xx)

	/// <summary>
	/// 错误的请求
	/// </summary>
	public const string BadRequest = "400";

	/// <summary>
	/// 参数验证失败
	/// </summary>
	public const string ValidationError = "40001";

	/// <summary>
	/// 参数缺失
	/// </summary>
	public const string MissingParameter = "40002";

	/// <summary>
	/// 参数格式错误
	/// </summary>
	public const string InvalidParameter = "40003";

	/// <summary>
	/// 请求体为空
	/// </summary>
	public const string EmptyRequestBody = "40004";

	#endregion

	#region 认证错误 (401xx) - 与登录/Token相关

	/// <summary>
	/// 未认证（需要登录）
	/// </summary>
	public const string Unauthorized = "401";

	/// <summary>
	/// Access Token 已过期（可通过 Refresh Token 刷新）
	/// </summary>
	public const string AccessTokenExpired = "40101";

	/// <summary>
	/// Refresh Token 已过期（需要重新登录）
	/// </summary>
	public const string RefreshTokenExpired = "40102";

	/// <summary>
	/// Token 无效或已被撤销
	/// </summary>
	public const string TokenInvalid = "40103";

	/// <summary>
	/// 用户名或密码错误
	/// </summary>
	public const string InvalidCredentials = "40104";

	/// <summary>
	/// 账户已被锁定（登录失败次数过多）
	/// </summary>
	public const string AccountLocked = "40105";

	/// <summary>
	/// 账户已被禁用
	/// </summary>
	public const string AccountDisabled = "40106";

	/// <summary>
	/// 账户未激活
	/// </summary>
	public const string AccountNotActivated = "40107";

	/// <summary>
	/// 需要二次验证
	/// </summary>
	public const string TwoFactorRequired = "40108";

	/// <summary>
	/// 二次验证码错误
	/// </summary>
	public const string TwoFactorInvalid = "40109";

	/// <summary>
	/// 强制登出（如：密码已修改、被管理员踢出）
	/// </summary>
	public const string ForceLogout = "40110";

	/// <summary>
	/// 会话已过期
	/// </summary>
	public const string SessionExpired = "40111";

	/// <summary>
	/// 登录设备数量超限
	/// </summary>
	public const string TooManyDevices = "40112";

	#endregion

	#region 授权错误 (403xx) - 与权限相关

	/// <summary>
	/// 禁止访问（无权限）
	/// </summary>
	public const string Forbidden = "403";

	/// <summary>
	/// 缺少必要的角色
	/// </summary>
	public const string InsufficientRole = "40301";

	/// <summary>
	/// 缺少必要的权限
	/// </summary>
	public const string InsufficientPermission = "40302";

	/// <summary>
	/// 资源访问受限
	/// </summary>
	public const string ResourceRestricted = "40303";

	/// <summary>
	/// IP 访问受限
	/// </summary>
	public const string IpRestricted = "40304";

	/// <summary>
	/// 操作时间受限
	/// </summary>
	public const string TimeRestricted = "40305";

	#endregion

	/// <summary>
	/// 方法不允许
	/// </summary>
	public const string MethodNotAllowed = "405";

	#region 资源错误 (404xx)

	/// <summary>
	/// 资源不存在
	/// </summary>
	public const string NotFound = "404";

	/// <summary>
	/// 用户不存在
	/// </summary>
	public const string AccountNotFound = "40401";

	/// <summary>
	/// 数据不存在
	/// </summary>
	public const string DataNotFound = "40402";

	/// <summary>
	/// 接口不存在
	/// </summary>
	public const string ApiNotFound = "40403";

	#endregion

	#region 业务错误 (409xx) - 冲突/业务规则

	/// <summary>
	/// 业务冲突
	/// </summary>
	public const string Conflict = "409";

	/// <summary>
	/// 数据已存在
	/// </summary>
	public const string DataAlreadyExists = "40901";

	/// <summary>
	/// 用户名已被使用
	/// </summary>
	public const string UsernameExists = "40902";

	/// <summary>
	/// 邮箱已被使用
	/// </summary>
	public const string EmailExists = "40903";

	/// <summary>
	/// 手机号已被使用
	/// </summary>
	public const string PhoneExists = "40904";

	/// <summary>
	/// 操作冲突（如并发修改）
	/// </summary>
	public const string OperationConflict = "40905";

	#endregion

	#region 请求限制 (429xx)

	/// <summary>
	/// 请求过于频繁
	/// </summary>
	public const string TooManyRequests = "429";

	/// <summary>
	/// API 调用次数超限
	/// </summary>
	public const string RateLimitExceeded = "42901";

	/// <summary>
	/// 操作过于频繁
	/// </summary>
	public const string OperationTooFrequent = "42902";

	#endregion

	#region 服务器错误 (500xx)

	/// <summary>
	/// 服务器内部错误
	/// </summary>
	public const string InternalError = "500";

	/// <summary>
	/// 数据库错误
	/// </summary>
	public const string DatabaseError = "50001";

	/// <summary>
	/// 缓存服务错误
	/// </summary>
	public const string CacheError = "50002";

	/// <summary>
	/// 文件服务错误
	/// </summary>
	public const string FileServiceError = "50003";

	/// <summary>
	/// 外部服务调用失败
	/// </summary>
	public const string ExternalServiceError = "50004";

	/// <summary>
	/// 配置错误
	/// </summary>
	public const string ConfigurationError = "50005";

	#endregion

	#region 服务不可用 (503xx)

	/// <summary>
	/// 服务不可用
	/// </summary>
	public const string ServiceUnavailable = "503";

	/// <summary>
	/// 服务维护中
	/// </summary>
	public const string ServiceMaintenance = "50301";

	/// <summary>
	/// 服务过载
	/// </summary>
	public const string ServiceOverloaded = "50302";

	#endregion
}

/// <summary>
/// 状态码分类助手
/// </summary>
public static class ApiResponseCodeHelper
{
	/// <summary>
	/// 是否为成功状态码
	/// </summary>
	public static bool IsSuccess(string code) => code == ApiResponseCode.Success;

	/// <summary>
	/// 是否为认证错误（401xx）
	/// </summary>
	public static bool IsAuthenticationError(string code) =>
		code.StartsWith("401") || code == "401";

	/// <summary>
	/// 是否为授权错误（403xx）
	/// </summary>
	public static bool IsAuthorizationError(string code) =>
		code.StartsWith("403") || code == "403";

	/// <summary>
	/// 是否为可通过刷新 Token 恢复的错误
	/// </summary>
	public static bool IsTokenRefreshable(string code) =>
		code == ApiResponseCode.AccessTokenExpired;

	/// <summary>
	/// 是否需要重新登录
	/// </summary>
	public static bool RequiresReLogin(string code) => code is
		ApiResponseCode.RefreshTokenExpired or
		ApiResponseCode.TokenInvalid or
		ApiResponseCode.AccountLocked or
		ApiResponseCode.AccountDisabled or
		ApiResponseCode.ForceLogout or
		ApiResponseCode.SessionExpired;

	/// <summary>
	/// 是否需要弹窗提示后登出
	/// </summary>
	public static bool RequiresModalLogout(string code) => code is
		ApiResponseCode.AccountDisabled or
		ApiResponseCode.ForceLogout or
		ApiResponseCode.TooManyDevices;
}