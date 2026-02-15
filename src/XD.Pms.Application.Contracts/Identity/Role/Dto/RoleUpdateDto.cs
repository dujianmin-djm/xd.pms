using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities;

namespace XD.Pms.Identity.Role.Dto;

public class RoleUpdateDto : IHasConcurrencyStamp
{
	[Required]
	[StringLength(256)]
	public string Name { get; set; } = default!;

	[Required]
	[StringLength(50)]
	public string Number { get; set; } = default!;

	[StringLength(256)]
	public string Description { get; set; } = string.Empty;

	public bool IsActive { get; set; }

	public bool IsDefault { get; set; }

	public bool IsPublic { get; set; }

	public string ConcurrencyStamp { get; set; } = default!;
}
