using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;
using XD.Pms.Enums;

namespace XD.Pms.Identity.User.Dto;

public class UserDto : FullAuditedEntityDto<Guid>, IHasConcurrencyStamp
{
	public string? UserName { get; set; }
	public string? Email { get; set; }
	public string? PhoneNumber { get; set; }
	public Gender Gender { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }
	public bool LockoutEnabled { get; set; }
	public int AccessFailedCount { get; set; }
	public DateTimeOffset? LockoutEnd { get; set; }
	public string ConcurrencyStamp { get; set; } = default!;
	public DateTimeOffset? LastPasswordChangeTime { get; set; }
	public DateTimeOffset? LastSignInTime { get; set; }
	public string? CreatedBy { get; set; }
	public string? LastModifiedBy { get; set; }
}
