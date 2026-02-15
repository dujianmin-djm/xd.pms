using System.ComponentModel.DataAnnotations;
using Volo.Abp.Identity;
using XD.Pms.Enums;

namespace XD.Pms.Identity.User.Dto;

public class UserCreateDto : IdentityUserCreateDto
{
	[StringLength(256)]
	public string Description { get; set; } = string.Empty;

	public Gender? Gender { get; set; }
}
