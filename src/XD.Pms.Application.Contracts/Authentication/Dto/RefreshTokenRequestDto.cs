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
	[Required]
	public string RefreshToken { get; set; } = default!;

	/// <summary>
	/// 客户端标识
	/// </summary>
	public string? ClientId { get; set; }
}
