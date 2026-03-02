using System;
using Volo.Abp.Domain.Entities;
using XD.Pms.BaseData.Departments;
using XD.Pms.BaseData.Positions;

namespace XD.Pms.BaseData.Employees;

public class EmployeePosition : Entity<Guid>
{
	public Guid EmployeeId { get; private set; }
	public Guid DepartmentId { get; private set; }
	public Guid PositionId { get; private set; }
	public DateTime StartDate { get; private set; }
	public bool IsPrimary { get; private set; }

	public Department? Department { get; private set; }
	public Position? Position { get; private set; }

	private EmployeePosition() { }

	internal EmployeePosition(
		Guid id, 
		Guid employeeId, 
		Guid departmentId, 
		Guid positionId,
		DateTime startDate, 
		bool isPrimary) : base(id)
	{
		EmployeeId = employeeId;
		DepartmentId = departmentId;
		PositionId = positionId;
		StartDate = startDate;
		IsPrimary = isPrimary;
	}

	internal void Update(Guid departmentId, Guid positionId, DateTime startDate, bool isPrimary)
	{
		DepartmentId = departmentId;
		PositionId = positionId;
		StartDate = startDate;
		IsPrimary = isPrimary;
	}

	internal void SetIsPrimary(bool isPrimary) => IsPrimary = isPrimary;
}
