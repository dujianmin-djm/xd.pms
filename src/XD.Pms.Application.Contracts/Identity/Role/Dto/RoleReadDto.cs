using XD.Pms.Services.Dtos;

namespace XD.Pms.Identity.Role.Dto;

public class RoleReadDto : PagedRequestDto
{
	public string? Number { get; set; }
	public string? Name { get; set; }
	public bool? IsActive { get; set; }
	public string? Filter { get; set; }
}
