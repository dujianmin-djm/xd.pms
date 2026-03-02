using System;
using System.ComponentModel.DataAnnotations;

namespace XD.Pms.BaseData.Positions.Dto;

public class PositionCreateDto
{
	[Required, MaxLength(50)]
	public string Number { get; set; } = string.Empty;

	[Required, MaxLength(100)]
	public string Name { get; set; } = string.Empty;

	[MaxLength(512)]
	public string? Description { get; set; }

	[Required]
	public Guid DepartmentId { get; set; }

	public bool IsLeader { get; set; }
}
