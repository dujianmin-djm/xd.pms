using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Data;
using XD.Pms.BaseData.Departments;
using XD.Pms.BaseData.Departments.Dto;
using XD.Pms.BaseData.Positions;
using XD.Pms.Permissions;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData;

[Route("papi/department")]
[Authorize(PmsPermissions.BaseData.Departments.Default)]
public class DepartmentAppService : PmsAppService, IDepartmentAppService
{
	private readonly IDepartmentRepository _departmentRepository;
	private readonly DepartmentManager _departmentManager;
	private readonly IPositionRepository _positionRepository;

	public DepartmentAppService(
		IDepartmentRepository departmentRepository,
		DepartmentManager departmentManager,
		IPositionRepository positionRepository)
	{
		_departmentRepository = departmentRepository;
		_departmentManager = departmentManager;
		_positionRepository = positionRepository;
	}

	[HttpGet("{id}")]
	public async Task<DepartmentDto> GetAsync(Guid id)
	{
		var entity = await _departmentRepository.GetAsync(id);
		return ObjectMapper.Map<Department, DepartmentDto>(entity);
	}

	[HttpGet("query")]
	public async Task<PagedResponseDto<DepartmentDto>> GetListAsync(DepartmentReadDto input)
	{
		int skip = (input.Current - 1) * input.Size;
		var list = await _departmentRepository.GetListAsync(
			input.Number, input.Name, input.DocumentStatus, input.Sorts, skip, input.Size);
		long total = await _departmentRepository.GetCountAsync(
			input.Number, input.Name, input.DocumentStatus);

		return new PagedResponseDto<DepartmentDto>(
			total, ObjectMapper.Map<List<Department>, List<DepartmentDto>>(list))
		{
			Current = input.Current,
			Size = input.Size,
			Sorts = input.Sorts
		};
	}

	[HttpGet("lookup")]
	public async Task<List<DepartmentLookupDto>> GetLookupAsync()
	{
		var list = await _departmentRepository.GetAllAsync();
		return ObjectMapper.Map<List<Department>, List<DepartmentLookupDto>>(list);
	}

	[HttpPost("add")]
	[Authorize(PmsPermissions.BaseData.Departments.Create)]
	public async Task<DepartmentDto> CreateAsync(DepartmentCreateDto input)
	{
		await _departmentManager.CheckNumberDuplicateAsync(input.Number);
		var fullName = await _departmentManager.ComputeFullNameAsync(input.ParentId, input.Name);

		var entity = new Department(
			GuidGenerator.Create(),
			input.Number, input.Name, input.ParentId, fullName, input.Description);

		await _departmentRepository.InsertAsync(entity, autoSave: true);
		return ObjectMapper.Map<Department, DepartmentDto>(entity);
	}

	[HttpPut("edit/{id}")]
	[Authorize(PmsPermissions.BaseData.Departments.Update)]
	public async Task<DepartmentDto> UpdateAsync(Guid id, DepartmentUpdateDto input)
	{
		var entity = await _departmentRepository.GetAsync(id);
		entity.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

		await _departmentManager.CheckNumberDuplicateAsync(input.Number, id);
		await _departmentManager.CheckParentNotSelfOrDescendantAsync(id, input.ParentId);

		var fullName = await _departmentManager.ComputeFullNameAsync(input.ParentId, input.Name);
		entity.Update(input.Number, input.Name, input.ParentId, fullName, input.Description);

		// 级联更新子孙部门的全称
		await _departmentManager.UpdateSubtreeFullNameAsync(entity);

		await _departmentRepository.UpdateAsync(entity, autoSave: true);
		return ObjectMapper.Map<Department, DepartmentDto>(entity);
	}

	[HttpDelete("delete/{id}")]
	[Authorize(PmsPermissions.BaseData.Departments.Delete)]
	public async Task DeleteAsync(Guid id)
	{
		var entity = await _departmentRepository.GetAsync(id);
		entity.EnsureDeletable();
		await _departmentManager.CheckDeletableAsync(id);

		// 检查岗位引用
		if (await _positionRepository.AnyByDepartmentAsync(id))
			throw new BusinessException(PmsDomainErrorCodes.DepartmentCannotDeleteReferenced);

		await _departmentRepository.DeleteAsync(entity);
	}

	[HttpPost("submit/{id}")]
	[Authorize(PmsPermissions.BaseData.Departments.Submit)]
	public async Task SubmitAsync(Guid id)
	{
		var entity = await _departmentRepository.GetAsync(id);
		entity.Submit();
		await _departmentRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("cancel/{id}")]
	[Authorize(PmsPermissions.BaseData.Departments.Cancel)]
	public async Task CancelAsync(Guid id)
	{
		var entity = await _departmentRepository.GetAsync(id);
		entity.UnAudit();
		await _departmentRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("audit/{id}")]
	[Authorize(PmsPermissions.BaseData.Departments.Audit)]
	public async Task AuditAsync(Guid id)
	{
		var entity = await _departmentRepository.GetAsync(id);
		entity.Audit(CurrentUser.Id!.Value, Clock.Now);
		await _departmentRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("unaudit/{id}")]
	[Authorize(PmsPermissions.BaseData.Departments.UnAudit)]
	public async Task UnAuditAsync(Guid id)
	{
		var entity = await _departmentRepository.GetAsync(id);
		entity.UnAudit();
		await _departmentRepository.UpdateAsync(entity, autoSave: true);
	}
}
