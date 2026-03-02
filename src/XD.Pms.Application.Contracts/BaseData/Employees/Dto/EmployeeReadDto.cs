using XD.Pms.Enums;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData.Employees.Dto;

public class EmployeeReadDto : PagedRequestDto
{
	public string? Number { get; set; }
	public string? Name { get; set; }
	public Gender? Gender { get; set; }
	public string? Phone { get; set; }
	public DocumentStatus? DocumentStatus { get; set; }
}
