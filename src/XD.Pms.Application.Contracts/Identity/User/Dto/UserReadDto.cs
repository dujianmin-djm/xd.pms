using XD.Pms.Enums;
using XD.Pms.Services.Dtos;

namespace XD.Pms.Identity.User.Dto;

public class UserReadDto : PagedRequestDto
{
	public string? UserName { get; set; }
	public Gender? Gender { get; set; }
	public string? PhoneNumber { get; set; }
	public string? Email { get; set; }
	public bool? IsActive { get; set; }
}
