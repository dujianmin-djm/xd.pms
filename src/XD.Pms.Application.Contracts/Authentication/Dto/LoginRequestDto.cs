using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Authentication.Dto;

/// <summary>
/// 登录请求
/// </summary>
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
	/// 客户端标识（"Pms_App", "Pms_Mobile"）
	/// </summary>
	public string? ClientId { get; set; }

	/// <summary>
	/// 请求的权限范围
	/// </summary>
	public string? Scope { get; set; }
}
