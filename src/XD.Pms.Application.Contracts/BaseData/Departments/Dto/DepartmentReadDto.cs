using XD.Pms.Enums;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData.Departments.Dto;

public class DepartmentReadDto : PagedRequestDto
{
	public string? Number { get; set; }
	public string? Name { get; set; }
	public DocumentStatus? DocumentStatus { get; set; }
}
