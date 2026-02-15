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
	[Required]
	[StringLength(128)]
	public string UserNameOrEmail { get; set; } = default!;

	/// <summary>
	/// 密码（RSA 加密后的 Base64 字符串）
	/// </summary>
	[Required]
	[StringLength(512)]
	public string Password { get; set; } = default!;

	/// <summary>
	/// 密码是否加密（默认 true）
	/// </summary>
	public bool IsEncrypted { get; set; } = true;

	/// <summary>
	/// 客户端标识
	/// </summary>
	public string? ClientId { get; set; }

	/// <summary>
	/// 请求的权限范围
	/// </summary>
	public string? Scope { get; set; }
}
