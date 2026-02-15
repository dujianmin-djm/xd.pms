using System;
using System.Collections.Generic;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace XD.Pms.ApiKeys;

/// <summary>
/// API Key 实体
/// </summary>
public class ApiKey : FullAuditedAggregateRoot<Guid>
{
	/// <summary>
	/// API Key 哈希值（SHA256，不存储明文）
	/// </summary>
	public string KeyHash { get; private set; } = default!;

	/// <summary>
	/// Key 前缀（用于显示识别，如 "pk_xxxx..."）
	/// </summary>
	public string KeyPrefix { get; private set; } = default!;

	/// <summary>
	/// 客户端 ID（唯一标识）
	/// </summary>
	public string ClientId { get; private set; } = default!;

	/// <summary>
	/// 客户端名称
	/// </summary>
	public string ClientName { get; private set; } = default!;

	/// <summary>
	/// 描述
	/// </summary>
	public string? Description { get; private set; }

	/// <summary>
	/// 是否激活
	/// </summary>
	public bool IsActive { get; private set; } = true;

	/// <summary>
	/// 过期时间（null 表示永不过期）
	/// </summary>
	public DateTime? ExpiresAt { get; private set; }

	/// <summary>
	/// 最后使用时间
	/// </summary>
	public DateTime? LastUsedAt { get; private set; }

	/// <summary>
	/// 最后使用 IP
	/// </summary>
	public string? LastUsedIp { get; private set; }

	/// <summary>
	/// 使用次数
	/// </summary>
	public long UsageCount { get; private set; }

	/// <summary>
	/// 角色（逗号分隔）
	/// </summary>
	public string? Roles { get; private set; }

	/// <summary>
	/// 权限（逗号分隔）
	/// </summary>
	public string? Permissions { get; private set; }

	/// <summary>
	/// 允许的 IP 地址（逗号分隔，空表示不限制）
	/// </summary>
	public string? AllowedIpAddresses { get; private set; }

	/// <summary>
	/// 每分钟请求限制（0 表示不限制）
	/// </summary>
	public int RateLimitPerMinute { get; private set; }

	/// <summary>
	/// 关联用户 ID
	/// </summary>
	public Guid? UserId { get; private set; }

	protected ApiKey()
	{
	}

	public ApiKey(
		Guid id,
		string keyHash,
		string keyPrefix,
		string clientId,
		string clientName,
		Guid? userId = null)
		: base(id)
	{
		SetKeyHash(keyHash);
		SetKeyPrefix(keyPrefix);
		SetClientId(clientId);
		SetClientName(clientName);
		UserId = userId;
	}

	#region Setters

	internal ApiKey SetKeyHash(string keyHash)
	{
		KeyHash = Check.NotNullOrWhiteSpace(keyHash, nameof(keyHash), ApiKeyConsts.KeyHashLength);
		return this;
	}

	internal ApiKey SetKeyPrefix(string keyPrefix)
	{
		KeyPrefix = Check.NotNullOrWhiteSpace(keyPrefix, nameof(keyPrefix), ApiKeyConsts.MaxKeyPrefixLength);
		return this;
	}

	public ApiKey SetClientId(string clientId)
	{
		ClientId = Check.NotNullOrWhiteSpace(clientId, nameof(clientId), ApiKeyConsts.MaxClientIdLength);
		return this;
	}

	public ApiKey SetClientName(string clientName)
	{
		ClientName = Check.NotNullOrWhiteSpace(clientName, nameof(clientName), ApiKeyConsts.MaxClientNameLength);
		return this;
	}

	public ApiKey SetDescription(string? description)
	{
		Description = Check.Length(description, nameof(description), ApiKeyConsts.MaxDescriptionLength);
		return this;
	}

	public ApiKey SetActive(bool isActive)
	{
		IsActive = isActive;
		return this;
	}

	public ApiKey SetExpiresAt(DateTime? expiresAt)
	{
		ExpiresAt = expiresAt;
		return this;
	}

	public ApiKey SetRoles(IEnumerable<string>? roles)
	{
		Roles = roles != null ? string.Join(",", roles) : null;
		return this;
	}

	public ApiKey SetPermissions(IEnumerable<string>? permissions)
	{
		Permissions = permissions != null ? string.Join(",", permissions) : null;
		return this;
	}

	public ApiKey SetAllowedIpAddresses(IEnumerable<string>? ipAddresses)
	{
		AllowedIpAddresses = ipAddresses != null ? string.Join(",", ipAddresses) : null;
		return this;
	}

	public ApiKey SetRateLimitPerMinute(int limit)
	{
		RateLimitPerMinute = Math.Max(0, limit);
		return this;
	}

	public ApiKey SetUserId(Guid? userId)
	{
		UserId = userId;
		return this;
	}

	#endregion

	#region Methods

	/// <summary>
	/// 记录使用
	/// </summary>
	public void RecordUsage(string? ipAddress = null)
	{
		LastUsedAt = DateTime.UtcNow;
		LastUsedIp = ipAddress;
		UsageCount++;
	}

	/// <summary>
	/// 激活
	/// </summary>
	public void Activate()
	{
		IsActive = true;
	}

	/// <summary>
	/// 禁用
	/// </summary>
	public void Deactivate()
	{
		IsActive = false;
	}

	/// <summary>
	/// 获取角色列表
	/// </summary>
	public List<string> GetRoles()
	{
		if (string.IsNullOrWhiteSpace(Roles))
		{
			return [];
		}

		return [.. Roles.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
	}

	/// <summary>
	/// 获取权限列表
	/// </summary>
	public List<string> GetPermissions()
	{
		if (string.IsNullOrWhiteSpace(Permissions))
		{
			return [];
		}

		return [.. Permissions.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
	}

	/// <summary>
	/// 获取允许的 IP 地址列表
	/// </summary>
	public List<string> GetAllowedIpAddresses()
	{
		if (string.IsNullOrWhiteSpace(AllowedIpAddresses))
		{
			return [];
		}

		return [.. AllowedIpAddresses.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
	}

	/// <summary>
	/// 检查是否过期
	/// </summary>
	public bool IsExpired()
	{
		return ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow;
	}

	/// <summary>
	/// 检查 IP 是否允许
	/// </summary>
	public bool IsIpAllowed(string? ipAddress)
	{
		var allowedIps = GetAllowedIpAddresses();

		// 没有配置 IP 限制，允许所有
		if (allowedIps.Count == 0)
		{
			return true;
		}

		if (string.IsNullOrWhiteSpace(ipAddress))
		{
			return false;
		}

		return allowedIps.Contains(ipAddress);
	}

	#endregion
}