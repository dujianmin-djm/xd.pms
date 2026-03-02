using System;
using Volo.Abp;
using XD.Pms.Common;

namespace XD.Pms.BaseData.Departments;

public class Department : PmsAuditedAggregateRoot
{
	public string Number { get; private set; } = string.Empty;
	public string Name { get; private set; } = string.Empty;
	public string? Description { get; private set; }
	public Guid? ParentId { get; private set; }
	public string FullName { get; private set; } = string.Empty;

	public Department? Parent { get; private set; }

	private Department() { }

	public Department(Guid id, string number, string name, Guid? parentId, string fullName,
					  string? description = null) : base()
	{
		Id = id;
		SetNumber(number);
		SetName(name);
		SetParent(parentId, fullName);
		Description = description?.Trim();
	}

	public void Update(string number, string name, Guid? parentId, string fullName, string? description)
	{
		CheckEditable();
		SetNumber(number);
		SetName(name);
		SetParent(parentId, fullName);
		Description = description?.Trim();
	}

	public virtual void SetNumber(string number)
	{
		Number = Check.NotNullOrWhiteSpace(number, nameof(number), 50);
	}

	public void SetName(string name)
	{
		Name = Check.NotNullOrWhiteSpace(name, nameof(name), 100);
	}

	public void SetParent(Guid? parentId, string fullName)
	{
		ParentId = parentId;
		FullName = Check.NotNullOrWhiteSpace(fullName, nameof(fullName));
	}

	public void SetFullName(string fullName)
	{
		FullName = Check.NotNullOrWhiteSpace(fullName, nameof(fullName), 1024);
	}

	public void EnsureDeletable()
	{
		CheckDeletable();
	}
}