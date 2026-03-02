using System;
using XD.Pms.Enums;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData.Positions.Dto;

public class PositionReadDto : PagedRequestDto
{
	public string? Number { get; set; }
	public string? Name { get; set; }
	public Guid? DepartmentId { get; set; }
	public DocumentStatus? DocumentStatus { get; set; }
}
