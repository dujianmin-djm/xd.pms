using System;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData.Departments.Dto;

public class DepartmentDto : PmsAuditedEntityDto
{
	public string Number { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public Guid? ParentId { get; set; }
	public string? ParentName { get; set; }
	public string FullName { get; set; } = string.Empty;
}

public class DepartmentLookupDto
{
	public Guid Id { get; set; }
	public string Number { get; set; } = string.Empty;
	public string Name { get; set; } = string.Empty;
	public string? Description { get; set; }
	public string? ParentName { get; set; }
	public string FullName { get; set; } = string.Empty;
}