using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Authentication.Dto;

/// <summary>
/// 刷新令牌请求
/// </summary>
public class RefreshTokenRequestDto
{
	/// <summary>
	/// 刷新令牌
	/// </summary>
	[Required(ErrorMessage = "刷新令牌不能为空")]
	public string RefreshToken { get; set; } = default!;

	/// <summary>
	/// 客户端标识（可选）
	/// </summary>
	public string? ClientId { get; set; }
}
