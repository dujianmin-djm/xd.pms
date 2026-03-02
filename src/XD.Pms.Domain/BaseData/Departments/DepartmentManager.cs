using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace XD.Pms.BaseData.Departments;

public class DepartmentManager : DomainService
{
	private readonly IDepartmentRepository _departmentRepository;

	public DepartmentManager(IDepartmentRepository departmentRepository)
	{
		_departmentRepository = departmentRepository;
	}

	/// <summary>
	/// 校验编码唯一
	/// </summary>
	public async Task CheckNumberDuplicateAsync(string number, Guid? excludeId = null)
	{
		var existing = await _departmentRepository.FindByNumberAsync(number);
		if (existing != null && existing.Id != excludeId)
			throw new BusinessException(PmsDomainErrorCodes.DuplicateNumber).WithData("Number", number);
	}

	/// <summary>
	/// 计算部门全称
	/// </summary>
	public async Task<string> ComputeFullNameAsync(Guid? parentId, string name)
	{
		if (parentId is null) return name;

		var parent = await _departmentRepository.FindAsync(parentId.Value);
		if (parent is null)
			throw new BusinessException(PmsDomainErrorCodes.DepartmentParentNotFound);

		return $"{parent.FullName}/{name}";
	}

	/// <summary>
	/// 更新自身及所有后代的全称
	/// </summary>
	public async Task UpdateSubtreeFullNameAsync(Department department)
	{
		var fullName = await ComputeFullNameAsync(department.ParentId, department.Name);
		department.SetFullName(fullName);

		var descendants = await _departmentRepository.GetDescendantsAsync(department.Id);
		// 按层级构建字典，逐层计算
		var dict = descendants.GroupBy(d => d.ParentId).ToDictionary(g => g.Key!.Value, g => g.ToList());
		await RecursiveUpdateFullName(department.Id, department.FullName, dict);
	}

	private static async Task RecursiveUpdateFullName(
		Guid parentId, 
		string parentFullName,
		Dictionary<Guid, List<Department>> dict)
	{
		if (!dict.TryGetValue(parentId, out var children)) return;
		foreach (var child in children)
		{
			var fn = $"{parentFullName}/{child.Name}";
			child.SetFullName(fn);
			await RecursiveUpdateFullName(child.Id, fn, dict);
		}
	}

	/// <summary>
	/// 校验能否删除
	/// </summary>
	public async Task CheckDeletableAsync(Guid id)
	{
		if (await _departmentRepository.HasChildrenAsync(id))
			throw new BusinessException(PmsDomainErrorCodes.DepartmentCannotDeleteHasChildren);
	}

	/// <summary>
	/// 校验不能将自身或后代设为上级
	/// </summary>
	public async Task CheckParentNotSelfOrDescendantAsync(Guid id, Guid? parentId)
	{
		if (parentId is null) return;
		if (parentId == id)
			throw new BusinessException(PmsDomainErrorCodes.DepartmentCannotSetSelfAsParent);

		var descendants = await _departmentRepository.GetDescendantsAsync(id);
		if (descendants.Any(d => d.Id == parentId))
			throw new BusinessException(PmsDomainErrorCodes.DepartmentCannotSetSelfAsParent);
	}
}