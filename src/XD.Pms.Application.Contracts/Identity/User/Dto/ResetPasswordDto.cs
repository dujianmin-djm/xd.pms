using System.ComponentModel.DataAnnotations;

namespace XD.Pms.Identity.User.Dto;

public class ResetPasswordDto
{
	[Required]
	[StringLength(128)]
	public required string Password { get; set; }
}