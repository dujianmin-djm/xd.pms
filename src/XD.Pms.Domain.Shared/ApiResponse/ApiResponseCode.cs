namespace XD.Pms.ApiResponse;

/// <summary>
/// API 响应状态码
/// </summary>
public static class ApiResponseCode
{
	/// <summary>
	/// 成功
	/// </summary>
	public const string Success = "200";

	#region 前端特殊处理码 - 需与前端 .env 配置保持一致
	/// <summary>
	/// Access Token 已过期（前端会自动刷新 Token）
	/// 对应前端: VITE_SERVICE_EXPIRED_TOKEN_CODES
	/// </summary>
	public const string AccessTokenExpired = "9999";

	/// <summary>
	/// Refresh Token 过期或无效（需要重新登录）
	/// 对应前端: VITE_SERVICE_EXPIRED_TOKEN_CODES
	/// </summary>
	public const string RefreshTokenExpired = "9998";

	/// <summary>
	/// 会话已失效，需要重新登录（静默跳转登录页）
	/// Refresh Token 已被使用（可能存在重放攻击，需重新登录）
	/// 对应前端: VITE_SERVICE_LOGOUT_CODES
	/// </summary>
	public const string RefreshTokenRedeemed = "8888";

	/// <summary>
	/// Token 已被撤销（静默跳转登录页）
	/// 对应前端: VITE_SERVICE_LOGOUT_CODES
	/// </summary>
	public const string AccessTokenRevoked = "8889";

	/// <summary>
	/// 强制登出 - 密码已修改（弹窗提示后跳转）
	/// 对应前端: VITE_SERVICE_MODAL_LOGOUT_CODES
	/// </summary>
	public const string PasswordChanged = "7777";

	/// <summary>
	/// 强制登出 - 被管理员踢出（弹窗提示后跳转）
	/// 对应前端: VITE_SERVICE_MODAL_LOGOUT_CODES
	/// </summary>
	public const string KickedOut = "7778";
	#endregion

	/// <summary>
	/// 错误的请求
	/// </summary>
	public const string BadRequest = "400";

	/// <summary>
	/// 参数验证失败
	/// </summary>
	public const string ValidationError = "40001";

	#region 认证错误 (401xx) - 与登录/Token相关

	/// <summary>
	/// 未认证（需要登录）
	/// </summary>
	public const string Unauthorized = "401";

	/// <summary>
	/// Access Token 已过期（可通过 Refresh Token 刷新）
	/// </summary>
	//public const string AccessTokenExpired = "40101";

	/// <summary>
	/// Access Token 无效
	/// </summary>
	public const string AccessTokenInvalid = "40102";


	/// <summary>
	/// Refresh Token 无效
	/// </summary>
	public const string RefreshTokenInvalid = "40105";

	/// <summary>
	/// 用户名或密码错误
	/// </summary>
	public const string InvalidCredentials = "40106";

	/// <summary>
	/// 账户已被锁定（登录失败次数过多）
	/// </summary>
	public const string AccountLocked = "40107";

	/// <summary>
	/// 账户已被禁用
	/// </summary>
	public const string AccountDisabled = "40108";

	/// <summary>
	/// 强制登出（如：密码已修改、被管理员踢出）
	/// </summary>
	public const string ForceLogout = "40109";

	#endregion

	/// <summary>
	/// 禁止访问（无权限）
	/// </summary>
	public const string Forbidden = "403";

	/// <summary>
	/// 资源不存在
	/// </summary>
	public const string NotFound = "404";

	/// <summary>
	/// 方法不允许
	/// </summary>
	public const string MethodNotAllowed = "405";

	/// <summary>
	/// 业务冲突
	/// </summary>
	public const string Conflict = "409";

	/// <summary>
	/// 用户名已存在
	/// </summary>
	public const string UsernameExists = "40901";

	/// <summary>
	/// 邮箱已存在
	/// </summary>
	public const string EmailExists = "40902";

	/// <summary>
	/// 请求过于频繁
	/// </summary>
	public const string TooManyRequests = "429";

	/// <summary>
	/// 服务器内部错误
	/// </summary>
	public const string InternalError = "500";

	/// <summary>
	/// 服务不可用
	/// </summary>
	public const string ServiceUnavailable = "503";
}