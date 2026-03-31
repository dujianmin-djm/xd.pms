using Volo.Abp.Domain.Entities;

namespace XD.Pms.BaseData.Departments.Dto;

public class DepartmentUpdateDto : DepartmentCreateDto, IHasConcurrencyStamp
{
	public string ConcurrencyStamp { get; set; } = default!;
}
