using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XD.Pms.ApiKeys.Dto;

public class UpdateApiKeyInput
{
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
	/// 过期时间
	/// </summary>
	public DateTime? ExpiresAt { get; set; }

	/// <summary>
	/// 角色列表
	/// </summary>
	public List<string>? Roles { get; set; }

	/// <summary>
	/// 权限列表
	/// </summary>
	public List<string>? Permissions { get; set; }

	/// <summary>
	/// 允许的 IP 地址列表
	/// </summary>
	public List<string>? AllowedIpAddresses { get; set; }

	/// <summary>
	/// 每分钟请求限制
	/// </summary>
	public int RateLimitPerMinute { get; set; }
}