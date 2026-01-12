using System;

namespace XD.Pms.Authentication.Dto;

public class UserSessionDto
{
	public Guid TokenId { get; set; }
	public string? DeviceId { get; set; }
	public string? ClientIp { get; set; }
	public string? UserAgent { get; set; }
	public TokenType TokenType { get; set; }
	public DateTime CreatedAt { get; set; }
	public DateTime ExpiresAt { get; set; }
	public bool IsCurrent { get; set; }
}
