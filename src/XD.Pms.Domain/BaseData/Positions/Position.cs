using System;
using Volo.Abp;
using XD.Pms.BaseData.Departments;
using XD.Pms.Common;

namespace XD.Pms.BaseData.Positions;

public class Position : PmsAuditedAggregateRoot
{
	public string Number { get; private set; } = string.Empty;
	public string Name { get; private set; } = string.Empty;
	public string? Description { get; private set; }
	public Guid DepartmentId { get; private set; }
	public bool IsLeader { get; private set; }

	public Department? Department { get; private set; }

	private Position() { }

	public Position(Guid id, string number, string name, Guid departmentId, bool isLeader,
					string? description = null) : base()
	{
		Id = id;
		SetNumber(number);
		SetName(name);
		DepartmentId = departmentId;
		IsLeader = isLeader;
		Description = description?.Trim();
	}

	public void Update(string number, string name, Guid departmentId, bool isLeader, string? description)
	{
		CheckEditable();
		SetNumber(number);
		SetName(name);
		DepartmentId = departmentId;
		IsLeader = isLeader;
		Description = description?.Trim();
	}

	public virtual void SetNumber(string number)
	{
		Number = Check.NotNullOrWhiteSpace(number, nameof(number));
	}

	public void SetName(string name)
	{
		Name = Check.NotNullOrWhiteSpace(name, nameof(name));
	}

	public void EnsureDeletable()
	{
		CheckDeletable();
	}
}
