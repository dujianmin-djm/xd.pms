using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Identity;
using XD.Pms.Identity.Role.Dto;
using XD.Pms.Identity.User.Dto;
using XD.Pms.Services;
using XD.Pms.Services.Dtos;

namespace XD.Pms.Identity.User;

public interface IUserAppService : ICrudAppService<UserDto, Guid, UserReadDto, UserCreateDto, UserUpdateDto>
{
	Task<ListResultDto<RoleDto>> GetRolesAsync(Guid id);
	Task UpdateRolesAsync(Guid id, IdentityUserUpdateRolesDto input);
	Task<UserDto> GetByUsernameAsync(string userName);
	Task<UserDto> GetByEmailAsync(string email);
}