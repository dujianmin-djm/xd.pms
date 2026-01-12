using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Authentication.Dto;

public class LoginRequestDto
{
	/// <summary>
	/// 用户名或邮箱
	/// </summary>
	[Required(ErrorMessage = "用户名不能为空")]
	[StringLength(256)]
	public string UserNameOrEmail { get; set; } = default!;

	/// <summary>
	/// 密码
	/// </summary>
	[Required(ErrorMessage = "密码不能为空")]
	[StringLength(128)]
	public string Password { get; set; } = default!;

	/// <summary>
	/// 设备标识（可选，用于多设备管理）
	/// </summary>
	[StringLength(128)]
	public string? DeviceId { get; set; }

	/// <summary>
	/// Token类型
	/// </summary>
	[Required(ErrorMessage = "TokenType不能为空")]
	[Range(0, int.MaxValue, ErrorMessage = "TokenType值无效")]
	public TokenType TokenType { get; set; } = TokenType.Web;

	/// <summary>
	/// 记住我（延长RefreshToken有效期）
	/// </summary>
	public bool RememberMe { get; set; } = false;
}
