using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XD.Pms.Identity.Role.Dto;
using XD.Pms.Services;

namespace XD.Pms.Identity.Role;

public interface IRoleAppService : ICrudAppService<RoleDto, Guid, RoleReadDto, RoleCreateDto, RoleUpdateDto>
{
	Task<List<RoleDto>> GetAssignableRolesAsync();
	Task DeleteManyAsync(List<Guid> ids);
}
