using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Identity.Role.Dto;

public class RoleCreateDto
{
	[Required]
	[StringLength(128)]
	public string Name { get; set; } = default!;

	[Required]
	[StringLength(50)]
	public string Number { get; set; } = default!;

	[StringLength(256)]
	public string Description { get; set; } = string.Empty;

	public bool IsActive { get; set; }

	public bool IsDefault { get; set; }

	public bool IsPublic { get; set; }
}
