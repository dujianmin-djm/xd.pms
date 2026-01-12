using System;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace XD.Pms.Authentication;

/// <summary>
/// Refresh Token 实体
/// </summary>
public class RefreshToken : CreationAuditedEntity<Guid>, IMultiTenant
{
    /// <summary>
    /// 关联用户ID
    /// </summary>
    public Guid UserId { get; private set; }
    
    /// <summary>
    /// Token值
    /// </summary>
    public string Token { get;  set; }
    
    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTime ExpiresAt { get; private set; }
    
    /// <summary>
    /// 是否已撤销
    /// </summary>
    public bool IsRevoked { get; private set; }
    
    /// <summary>
    /// 撤销时间
    /// </summary>
    public DateTime? RevokedAt { get; private set; }
    
    /// <summary>
    /// 替代Token（当此Token被刷新后，指向新Token的ID）
    /// </summary>
    public Guid? ReplacedByTokenId { get; private set; }
    
    /// <summary>
    /// 客户端IP
    /// </summary>
    public string? ClientIp { get; private set; }
    
    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; private set; }
    
    /// <summary>
    /// 设备标识
    /// </summary>
    public string? DeviceId { get; private set; }
    
    /// <summary>
    /// Token类型
    /// </summary>
    public TokenType TokenType { get; private set; }
    
    /// <summary>
    /// 租户ID
    /// </summary>
    public Guid? TenantId { get; private set; }

	/// <summary>
	/// 是否有效（未过期且未撤销）
	/// </summary>
	public bool IsActive => !IsRevoked && ExpiresAt > DateTimeOffset.Now;
    
    public RefreshToken(
        Guid id,
        Guid userId,
        string token,
        DateTime expiresAt,
        TokenType tokenType,
		string? clientIp = null,
        string? userAgent = null,
        string? deviceId = null,
        Guid? tenantId = null)
    {
        Id = id;
        UserId = userId;
        Token = token;
        ExpiresAt = expiresAt;
        TokenType = tokenType;
        ClientIp = clientIp;
        UserAgent = userAgent;
        DeviceId = deviceId;
        TenantId = tenantId;
        IsRevoked = false;
	}
    
    /// <summary>
    /// 撤销Token
    /// </summary>
    public void Revoke(DateTime nowTime, Guid? replacedByTokenId = null)
    {
        IsRevoked = true;
        RevokedAt = nowTime;
        ReplacedByTokenId = replacedByTokenId;
    }
}
