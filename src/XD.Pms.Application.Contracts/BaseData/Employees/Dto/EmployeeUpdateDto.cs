using Volo.Abp.Domain.Entities;

namespace XD.Pms.BaseData.Employees.Dto;

public class EmployeeUpdateDto : EmployeeCreateDto, IHasConcurrencyStamp
{
	public string ConcurrencyStamp { get; set; } = default!;
}
