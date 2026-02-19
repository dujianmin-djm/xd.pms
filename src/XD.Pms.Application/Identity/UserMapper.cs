using Riok.Mapperly.Abstractions;
using System;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using XD.Pms.Enums;
using XD.Pms.Identity.User.Dto;

namespace XD.Pms.Identity;

[Mapper]
public partial class UserToUserDtoMapper : MapperBase<IdentityUser, UserDto>
{
	private readonly IIdentityUserRepository _userRepository;

	public UserToUserDtoMapper(IIdentityUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	[MapperIgnoreTarget(nameof(UserDto.Gender))]
	[MapperIgnoreTarget(nameof(UserDto.Description))]
	[MapperIgnoreTarget(nameof(UserDto.CreatedBy))]
	[MapperIgnoreTarget(nameof(UserDto.LastModifiedBy))]
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
	[MapperIgnoreSource(nameof(IdentityUser.PasswordHistories))]
	[MapperIgnoreSource(nameof(IdentityUser.Passkeys))]
	public override partial UserDto Map(IdentityUser source);

	[MapperIgnoreTarget(nameof(UserDto.Gender))]
	[MapperIgnoreTarget(nameof(UserDto.Description))]
	[MapperIgnoreTarget(nameof(UserDto.CreatedBy))]
	[MapperIgnoreTarget(nameof(UserDto.LastModifiedBy))]
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
	[MapperIgnoreSource(nameof(IdentityUser.PasswordHistories))]
	[MapperIgnoreSource(nameof(IdentityUser.Passkeys))]
	public override partial void Map(IdentityUser source, UserDto destination);

	public override void AfterMap(IdentityUser source, UserDto target)
	{
		target.Gender = source.GetProperty<Gender>("Gender");
		target.Description = source.GetProperty<string>("Description") ?? string.Empty;
		target.CreatedBy = GetUserName(source.CreatorId);
		target.LastModifiedBy = GetUserName(source.LastModifierId);
	}

	private string? GetUserName(Guid? userId)
	{
		if (!userId.HasValue) return null;
		var user = _userRepository.FindAsync(userId.Value).GetAwaiter().GetResult();
		return user?.UserName;
	}
}
