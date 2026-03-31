using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using XD.Pms.Enums;

namespace XD.Pms.BaseData.Employees.Dto;

public class EmployeeCreateDto
{
	[Required, MaxLength(50)]
	public string Number { get; set; } = string.Empty;

	[Required, MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(512)]
	public string? Description { get; set; }

	public DateTime? HireDate { get; set; }
	public Gender Gender { get; set; }

	[MaxLength(50)]
	public string? Phone { get; set; }

	[MaxLength(50)]
	public string? Email { get; set; }

	[MaxLength(256)]
	public string? Address { get; set; }

	public List<EmployeePositionCreateOrUpdateDto> Positions { get; set; } = [];
}
