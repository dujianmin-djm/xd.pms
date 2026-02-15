using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;

namespace XD.Pms.Identity.Role.Dto;

public class RoleDto : EntityDto<Guid>, IHasConcurrencyStamp, IHasCreationTime
{
	public string? Number { get; set; }
	public string? Name { get; set; }
	public string? Description { get; set; }
	public bool IsActive { get; set; }
	public bool IsDefault { get; set; }
	public bool IsStatic { get; set; }
	public bool IsPublic { get; set; }
	public string ConcurrencyStamp { get; set; } = default!;
	public DateTime CreationTime { get; set; }
}
