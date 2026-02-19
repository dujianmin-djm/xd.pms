using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Identity.User.Dto;

public class ChangePasswordDto
{
	[Required]
	public string CurrentPassword { get; set; } = string.Empty;

	[Required]
	[MinLength(6)]
	public string NewPassword { get; set; } = string.Empty;
}