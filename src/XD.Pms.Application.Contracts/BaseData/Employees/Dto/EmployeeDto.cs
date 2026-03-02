using System;
using System.Collections.Generic;
using XD.Pms.Enums;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData.Employees.Dto;

public class EmployeeDto : PmsAuditedEntityDto
{
	public string Number { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public DateTime? HireDate { get; set; }
	public Gender Gender { get; set; }
	public string? Phone { get; set; }
	public string? Email { get; set; }
	public string? Address { get; set; }
	public List<EmployeePositionDto> Positions { get; set; } = [];

	public string? PrimaryDepartmentName { get; set; }
	public string? PrimaryPositionName { get; set; }
}
