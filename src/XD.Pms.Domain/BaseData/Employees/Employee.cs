using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using XD.Pms.Common;
using XD.Pms.Enums;

namespace XD.Pms.BaseData.Employees;

public class Employee : PmsAuditedAggregateRoot
{
	public string Number { get; private set; } = string.Empty;
	public string Name { get; private set; } = string.Empty;
	public string? Description { get; private set; }
	public DateTime? HireDate { get; private set; }
	public Gender Gender { get; private set; }
	public string? Phone { get; private set; }
	public string? Email { get; private set; }
	public string? Address { get; private set; }

	public List<EmployeePosition> Positions { get; private set; } = [];

	private Employee() { }

	public Employee(
		Guid id, 
		string number, 
		string name, 
		Gender gender,
		DateTime? hireDate = null, 
		string? phone = null,
		string? email = null, 
		string? address = null,
		string? description = null) : base()
	{
		Id = id;
		SetNumber(number);
		SetName(name);
		Gender = gender;
		HireDate = hireDate;
		Phone = phone?.Trim();
		Email = email?.Trim();
		Address = address?.Trim();
		Description = description?.Trim();
	}

	public void Update(
		string number, 
		string name, 
		Gender gender,
		DateTime? hireDate, 
		string? phone, 
		string? email,
		string? address, 
		string? description)
	{
		CheckEditable();
		SetNumber(number);
		SetName(name);
		Gender = gender;
		HireDate = hireDate;
		Phone = phone?.Trim();
		Email = email?.Trim();
		Address = address?.Trim();
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

	/* ── 任岗明细管理 ── */
	public void AddPosition(Guid id, Guid departmentId, Guid positionId, DateTime startDate, bool isPrimary)
	{
		CheckEditable();
		if (isPrimary && Positions.Any(p => p.IsPrimary))
		{
			// 自动取消原主任岗
			foreach (var pa in Positions.Where(p => p.IsPrimary))
				pa.SetIsPrimary(false);
		}
		Positions.Add(new EmployeePosition(id, Id, departmentId, positionId, startDate, isPrimary));
	}

	public void UpdatePosition(Guid id, Guid departmentId, Guid positionId, DateTime startDate, bool isPrimary)
	{
		CheckEditable();
		var item = Positions.FirstOrDefault(p => p.Id == id)
				   ?? throw new BusinessException(PmsDomainErrorCodes.EmployeePositionNotFound);

		if (isPrimary)
		{
			foreach (var pa in Positions.Where(p => p.IsPrimary && p.Id != id))
				pa.SetIsPrimary(false);
		}
		item.Update(departmentId, positionId, startDate, isPrimary);
	}

	public void RemovePosition(Guid id)
	{
		CheckEditable();
		var item = Positions.FirstOrDefault(p => p.Id == id);
		if (item != null) Positions.Remove(item);
	}

	public void ClearPositions()
	{
		CheckEditable();
		Positions.Clear();
	}

	/// <summary>
	/// 替换全部任岗明细（用于编辑保存时的增删改合并）
	/// </summary>
	public void SyncPositions(
		List<(Guid? Id, Guid DepartmentId, Guid PositionId, DateTime StartDate, bool IsPrimary)> inputs,
		Func<Guid> guidGenerator)
	{
		CheckEditable();

		// 1. 校验主任岗数量
		if (inputs.Count(i => i.IsPrimary) > 1)
			throw new BusinessException(PmsDomainErrorCodes.EmployeeMultiplePrimary);

		// 2. 需要删除的
		var inputIds = inputs.Where(i => i.Id.HasValue).Select(i => i.Id!.Value).ToHashSet();
		Positions.RemoveAll(p => !inputIds.Contains(p.Id));

		// 3. 更新已有
		foreach (var input in inputs.Where(i => i.Id.HasValue))
		{
			var existing = Positions.FirstOrDefault(pa => pa.Id == input.Id!.Value);
			existing?.Update(input.DepartmentId, input.PositionId, input.StartDate, input.IsPrimary);
		}

		// 4. 新增
		foreach (var input in inputs.Where(i => !i.Id.HasValue))
		{
			Positions.Add(new EmployeePosition(
				guidGenerator(), Id, input.DepartmentId, input.PositionId, input.StartDate, input.IsPrimary));
		}
	}

	public void EnsureDeletable()
	{
		CheckDeletable();
	}
}
