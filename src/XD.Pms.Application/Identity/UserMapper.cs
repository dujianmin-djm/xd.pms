using Riok.Mapperly.Abstractions;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using XD.Pms.Enums;
using XD.Pms.Identity.User.Dto;

namespace XD.Pms.Identity;

[Mapper]
public partial class UserToUserDtoMapper : MapperBase<IdentityUser, UserDto>
{
	[MapperIgnoreTarget(nameof(UserDto.Gender))]
	[MapperIgnoreTarget(nameof(UserDto.Description))]
	[MapperIgnoreSource(nameof(IdentityUser.ExtraProperties))]
	[MapperIgnoreSource(nameof(IdentityUser.Name))]
	[MapperIgnoreSource(nameof(IdentityUser.Surname))]
	[MapperIgnoreSource(nameof(IdentityUser.TenantId))]
	[MapperIgnoreSource(nameof(IdentityUser.Claims))]
	[MapperIgnoreSource(nameof(IdentityUser.EntityVersion))]
	[MapperIgnoreSource(nameof(IdentityUser.NormalizedEmail))]
	[MapperIgnoreSource(nameof(IdentityUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(IdentityUser.EmailConfirmed))]
	[MapperIgnoreSource(nameof(IdentityUser.PhoneNumberConfirmed))]
	[MapperIgnoreSource(nameof(IdentityUser.PasswordHash))]
	[MapperIgnoreSource(nameof(IdentityUser.SecurityStamp))]
	[MapperIgnoreSource(nameof(IdentityUser.IsExternal))]
	[MapperIgnoreSource(nameof(IdentityUser.TwoFactorEnabled))]
	[MapperIgnoreSource(nameof(IdentityUser.ShouldChangePasswordOnNextLogin))]
	[MapperIgnoreSource(nameof(IdentityUser.Roles))]
	[MapperIgnoreSource(nameof(IdentityUser.Logins))]
	[MapperIgnoreSource(nameof(IdentityUser.Tokens))]
	[MapperIgnoreSource(nameof(IdentityUser.OrganizationUnits))]
	public override partial UserDto Map(IdentityUser source);

	[MapperIgnoreTarget(nameof(UserDto.Gender))]
	[MapperIgnoreTarget(nameof(UserDto.Description))]
	[MapperIgnoreSource(nameof(IdentityUser.ExtraProperties))]
	[MapperIgnoreSource(nameof(IdentityUser.Name))]
	[MapperIgnoreSource(nameof(IdentityUser.Surname))]
	[MapperIgnoreSource(nameof(IdentityUser.TenantId))]
	[MapperIgnoreSource(nameof(IdentityUser.Claims))]
	[MapperIgnoreSource(nameof(IdentityUser.EntityVersion))]
	[MapperIgnoreSource(nameof(IdentityUser.NormalizedEmail))]
	[MapperIgnoreSource(nameof(IdentityUser.NormalizedUserName))]
	[MapperIgnoreSource(nameof(IdentityUser.EmailConfirmed))]
	[MapperIgnoreSource(nameof(IdentityUser.PhoneNumberConfirmed))]
	[MapperIgnoreSource(nameof(IdentityUser.PasswordHash))]
	[MapperIgnoreSource(nameof(IdentityUser.SecurityStamp))]
	[MapperIgnoreSource(nameof(IdentityUser.IsExternal))]
	[MapperIgnoreSource(nameof(IdentityUser.TwoFactorEnabled))]
	[MapperIgnoreSource(nameof(IdentityUser.ShouldChangePasswordOnNextLogin))]
	[MapperIgnoreSource(nameof(IdentityUser.Roles))]
	[MapperIgnoreSource(nameof(IdentityUser.Logins))]
	[MapperIgnoreSource(nameof(IdentityUser.Tokens))]
	[MapperIgnoreSource(nameof(IdentityUser.OrganizationUnits))]
	public override partial void Map(IdentityUser source, UserDto destination);

	public override void AfterMap(IdentityUser source, UserDto target)
	{
		target.Gender = source.GetProperty<Gender>("Gender");
		target.Description = source.GetProperty<string>("Description") ?? string.Empty;
	}
}
