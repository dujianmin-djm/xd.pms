using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XD.Pms.ApiKeys.Dto;

public class CreateApiKeyInput
{
	/// <summary>
	/// 客户端 ID（唯一）
	/// </summary>
	[Required]
	[StringLength(ApiKeyConsts.MaxClientIdLength)]
	public string ClientId { get; set; } = default!;

	/// <summary>
	/// 客户端名称
	/// </summary>
	[Required]
	[StringLength(ApiKeyConsts.MaxClientNameLength)]
	public string ClientName { get; set; } = default!;

	/// <summary>
	/// 描述
	/// </summary>
	[StringLength(ApiKeyConsts.MaxDescriptionLength)]
	public string? Description { get; set; }

	/// <summary>
	/// 过期时间（null 表示永不过期）
	/// </summary>
	public DateTime? ExpiresAt { get; set; }

	/// <summary>
	/// 关联用户
	/// </summary>
	public Guid? UserId { get; set; }

	/// <summary>
	/// 角色列表
	/// </summary>
	public List<string>? Roles { get; set; }

	/// <summary>
	/// 权限列表
	/// </summary>
	public List<string>? Permissions { get; set; }

	/// <summary>
	/// 允许的 IP 地址列表（空表示不限制）
	/// </summary>
	public List<string>? AllowedIpAddresses { get; set; }

	/// <summary>
	/// 每分钟请求限制（0 表示不限制）
	/// </summary>
	public int RateLimitPerMinute { get; set; }

	/// <summary>
	/// Key 前缀
	/// </summary>
	[StringLength(10)]
	public string KeyPrefix { get; set; } = "pk";
}