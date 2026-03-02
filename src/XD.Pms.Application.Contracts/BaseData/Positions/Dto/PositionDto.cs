using System;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData.Positions.Dto;

public class PositionDto : PmsAuditedEntityDto
{
	public string Number { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid DepartmentId { get; set; }
	public string? DepartmentName { get; set; }
	public string? DepartmentFullName { get; set; }
	public bool IsLeader { get; set; }
}

public class PositionLookupDto
{
	public Guid Id { get; set; }
	public string Number { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? DepartmentName { get; set; }
	public string? DepartmentFullName { get; set; }
	public bool IsLeader { get; set; }
}