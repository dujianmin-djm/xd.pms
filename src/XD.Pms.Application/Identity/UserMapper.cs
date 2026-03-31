using Riok.Mapperly.Abstractions;
using System;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.Mapperly;
using XD.Pms.Enums;
using XD.Pms.Identity.User.Dto;

namespace XD.Pms.Identity;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class UserToUserDtoMapper : MapperBase<IdentityUser, UserDto>
{
	private readonly IIdentityUserRepository _userRepository;

	public UserToUserDtoMapper(IIdentityUserRepository userRepository)
	{
		_userRepository = userRepository;
	}

	public override partial UserDto Map(IdentityUser source);

	public override partial void Map(IdentityUser source, UserDto destination);

	public override void AfterMap(IdentityUser source, UserDto target)
	{
		target.Gender = source.GetProperty<Gender>("Gender");
		target.Description = source.GetProperty<string>("Description") ?? string.Empty;
		target.CreatorName = GetUserName(source.CreatorId);
		target.LastModifierName = GetUserName(source.LastModifierId);
	}

	private string? GetUserName(Guid? userId)
	{
		if (!userId.HasValue) return null;
		var user = _userRepository.FindAsync(userId.Value).GetAwaiter().GetResult();
		return user?.UserName;
	}
}
