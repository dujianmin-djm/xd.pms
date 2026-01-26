using XD.Pms.ApiResponse;

namespace XD.Pms.Authentication;

public class TokenAnalysisResult
{
	/// <summary>
	/// 错误代码
	/// </summary>
	public string Code { get; set; } = ApiResponseCode.Unauthorized;

	/// <summary>
	/// 错误消息
	/// </summary>
	public string Message { get; set; } = string.Empty;

	/// <summary>
	/// Token 状态
	/// </summary>
	public TokenStatus Status { get; set; }

	/// <summary>
	/// 详细信息（用于日志）
	/// </summary>
	public string? Details { get; set; }
}

public enum TokenStatus
{
	/// <summary>
	/// 未提供Token
	/// </summary>
	Missing,

	/// <summary>
	/// Token格式无效
	/// </summary>
	Invalid,

	/// <summary>
	/// Token已过期
	/// </summary>
	Expired,

	/// <summary>
	/// Token已被撤销（在黑名单中）
	/// </summary>
	Revoked,

	/// <summary>
	/// Token签名无效
	/// </summary>
	SignatureInvalid,

	/// <summary>
	/// 未知错误
	/// </summary>
	Unknown
}
