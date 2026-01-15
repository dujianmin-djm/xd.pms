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
	/// 邮箱是否已验证
	/// </summary>
	public bool EmailConfirmed { get; set; }

	/// <summary>
	/// 名字
	/// </summary>
	public string? Name { get; set; }

	/// <summary>
	/// 姓氏
	/// </summary>
	public string? Surname { get; set; }

	/// <summary>
	/// 电话号码
	/// </summary>
	public string? PhoneNumber { get; set; }

	/// <summary>
	/// 电话是否已验证
	/// </summary>
	public bool PhoneNumberConfirmed { get; set; }

	/// <summary>
	/// 是否启用双因素认证
	/// </summary>
	public bool TwoFactorEnabled { get; set; }

	/// <summary>
	/// 用户角色列表
	/// </summary>
	public string[] Roles { get; set; } = [];

	/// <summary>
	/// 租户ID
	/// </summary>
	public Guid? TenantId { get; set; }
}