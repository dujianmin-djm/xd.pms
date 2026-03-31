using System;
using Volo.Abp.Application.Dtos;

namespace XD.Pms.BaseData.Employees.Dto;

public class EmployeePositionDto : EntityDto<Guid>
{
	public Guid DepartmentId { get; set; }
	public string? DepartmentName { get; set; }
	public Guid PositionId { get; set; }
	public string? PositionName { get; set; }
	public DateTime StartDate { get; set; }
	public bool IsPrimary { get; set; }
}

public class EmployeePositionCreateOrUpdateDto
{
	public Guid? Id { get; set; }
	public Guid DepartmentId { get; set; }
	public Guid PositionId { get; set; }
	public DateTime StartDate { get; set; }
	public bool IsPrimary { get; set; }
}
