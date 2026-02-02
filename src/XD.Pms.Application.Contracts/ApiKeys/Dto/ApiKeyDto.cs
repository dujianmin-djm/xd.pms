using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace XD.Pms.ApiKeys.Dto;

public class ApiKeyDto : FullAuditedEntityDto<Guid>
{
	/// <summary>
	/// Key 前缀（用于显示，如 "pk_xxxx..."）
	/// </summary>
	public string KeyPrefix { get; set; } = default!;

	/// <summary>
	/// 客户端 ID
	/// </summary>
	public string ClientId { get; set; } = default!;

	/// <summary>
	/// 客户端名称
	/// </summary>
	public string ClientName { get; set; } = default!;

	/// <summary>
	/// 描述
	/// </summary>
	public string? Description { get; set; }

	/// <summary>
	/// 是否激活
	/// </summary>
	public bool IsActive { get; set; }

	/// <summary>
	/// 过期时间
	/// </summary>
	public DateTime? ExpiresAt { get; set; }

	/// <summary>
	/// 最后使用时间
	/// </summary>
	public DateTime? LastUsedAt { get; set; }

	/// <summary>
	/// 最后使用 IP
	/// </summary>
	public string? LastUsedIp { get; set; }

	/// <summary>
	/// 使用次数
	/// </summary>
	public long UsageCount { get; set; }

	/// <summary>
	/// 角色列表
	/// </summary>
	public List<string> Roles { get; set; } = [];

	/// <summary>
	/// 权限列表
	/// </summary>
	public List<string> Permissions { get; set; } = [];

	/// <summary>
	/// 允许的 IP 地址列表
	/// </summary>
	public List<string> AllowedIpAddresses { get; set; } = [];

	/// <summary>
	/// 每分钟请求限制
	/// </summary>
	public int RateLimitPerMinute { get; set; }

	/// <summary>
	/// 关联用户 ID
	/// </summary>
	public Guid? UserId { get; set; }

	/// <summary>
	/// 是否已过期
	/// </summary>
	public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
}