using System;

namespace XD.Pms.Authentication.Dto;

/// <summary>
/// 用户信息
/// </summary>
public class UserInfoDto
{
	/// <summary>
	/// 用户ID
	/// </summary>
	public Guid Id { get; set; }

	/// <summary>
	/// 用户名
	/// </summary>
	public string UserName { get; set; } = default!;

	/// <summary>
	/// 邮箱
	/// </summary>
	public string? Email { get; set; }

	/// <summary>
	/// 电话号码
	/// </summary>
	public string? PhoneNumber { get; set; }

	/// <summary>
	/// 用户角色列表
	/// </summary>
	public string[] Roles { get; set; } = [];

	/// <summary>
	/// 按钮权限列表
	/// </summary>
	public string[] Buttons { get; set; } = [];
}