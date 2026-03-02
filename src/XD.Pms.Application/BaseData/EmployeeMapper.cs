using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using XD.Pms.BaseData.Employees;
using XD.Pms.BaseData.Employees.Dto;

namespace XD.Pms.BaseData;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class EmployeeMapper : MapperBase<Employee, EmployeeDto>
{
	public override partial EmployeeDto Map(Employee source);

	public override partial void Map(Employee source, EmployeeDto destination);
}
