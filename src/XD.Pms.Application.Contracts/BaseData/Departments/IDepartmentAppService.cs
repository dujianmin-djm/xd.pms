using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XD.Pms.BaseData.Departments.Dto;
using XD.Pms.Services;

namespace XD.Pms.BaseData.Departments;

public interface IDepartmentAppService :
	ICrudAppService<DepartmentDto, Guid, DepartmentReadDto, DepartmentCreateDto, DepartmentUpdateDto>,
	IWorkflowAppService<Guid>
{
	Task<List<DepartmentLookupDto>> GetLookupAsync();
}
