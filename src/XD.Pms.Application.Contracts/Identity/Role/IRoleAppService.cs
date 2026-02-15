using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using XD.Pms.Identity.Role.Dto;
using XD.Pms.Services;

namespace XD.Pms.Identity.Role;

public interface IRoleAppService : ICrudAppService<RoleDto, Guid, RoleReadDto, RoleCreateDto, RoleUpdateDto>
{
	Task<ListResultDto<RoleDto>> GetAssignableRolesAsync();
	Task DeleteManyAsync(IEnumerable<Guid> ids);
}
