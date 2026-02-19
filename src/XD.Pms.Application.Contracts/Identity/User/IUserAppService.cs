using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using XD.Pms.Identity.Role.Dto;
using XD.Pms.Identity.User.Dto;
using XD.Pms.Services;

namespace XD.Pms.Identity.User;

public interface IUserAppService : ICrudAppService<UserDto, Guid, UserReadDto, UserCreateDto, UserUpdateDto>
{
	Task<List<RoleDto>> GetRolesAsync(Guid id);
	Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input);
	Task<UserDto> GetByUsernameAsync(string userName);
	Task<UserDto> GetByEmailAsync(string email);
	Task ResetPasswordAsync(Guid id, ResetPasswordDto input);
	Task ChangePasswordAsync(ChangePasswordDto input);
}