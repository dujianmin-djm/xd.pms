using System;
using System.Threading.Tasks;
using XD.Pms.BaseData.Employees.Dto;
using XD.Pms.Services;

namespace XD.Pms.BaseData.Employees;

public interface IEmployeeAppService : 
	ICrudAppService<EmployeeDto, Guid, EmployeeReadDto, EmployeeCreateDto, EmployeeUpdateDto>,
	IWorkflowAppService<Guid>
{

}