using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using XD.Pms.BaseData.Positions;
using XD.Pms.BaseData.Positions.Dto;
using XD.Pms.Permissions;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData;

[Route("papi/position")]
[Authorize(PmsPermissions.BaseData.Positions.Default)]
public class PositionAppService : PmsAppService, IPositionAppService
{
	private readonly IPositionRepository _positionRepository;
	private readonly PositionManager _positionManager;

	public PositionAppService(
		IPositionRepository positionRepository,
		PositionManager positionManager)
	{
		_positionRepository = positionRepository;
		_positionManager = positionManager;
	}

	[HttpGet("{id}")]
	public async Task<PositionDto> GetAsync(Guid id)
	{
		var entity = await _positionRepository.GetAsync(id);
		return ObjectMapper.Map<Position, PositionDto>(entity);
	}

	[HttpGet("query")]
	public async Task<PagedResponseDto<PositionDto>> GetListAsync(PositionReadDto input)
	{
		int skip = (input.Current - 1) * input.Size;
		var list = await _positionRepository.GetListAsync(
			input.Number, input.Name, input.DepartmentId, input.DocumentStatus,
			input.Sorts, skip, input.Size);
		long total = await _positionRepository.GetCountAsync(
			input.Number, input.Name, input.DepartmentId, input.DocumentStatus);

		return new PagedResponseDto<PositionDto>(
			total, ObjectMapper.Map<List<Position>, List<PositionDto>>(list))
		{
			Current = input.Current,
			Size = input.Size,
			Sorts = input.Sorts
		};
	}

	[HttpGet("lookup")]
	public async Task<List<PositionLookupDto>> GetLookupAsync([FromQuery] Guid? departmentId = null)
	{
		var list = await _positionRepository.GetAllAsync();
		if (departmentId.HasValue)
			list = list.Where(p => p.DepartmentId == departmentId.Value).ToList();
		return ObjectMapper.Map<List<Position>, List<PositionLookupDto>>(list);
	}

	[HttpPost("add")]
	[Authorize(PmsPermissions.BaseData.Positions.Create)]
	public async Task<PositionDto> CreateAsync(PositionCreateDto input)
	{
		await _positionManager.CheckNumberDuplicateAsync(input.Number);
		await _positionManager.CheckDepartmentExistsAsync(input.DepartmentId);

		var entity = new Position(
			GuidGenerator.Create(),
			input.Number, input.Name, input.DepartmentId, input.IsLeader, input.Description);

		await _positionRepository.InsertAsync(entity, autoSave: true);
		return ObjectMapper.Map<Position, PositionDto>(entity);
	}

	[HttpPut("edit/{id}")]
	[Authorize(PmsPermissions.BaseData.Positions.Update)]
	public async Task<PositionDto> UpdateAsync(Guid id, PositionUpdateDto input)
	{
		var entity = await _positionRepository.GetAsync(id);
		entity.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

		await _positionManager.CheckNumberDuplicateAsync(input.Number, id);
		await _positionManager.CheckDepartmentExistsAsync(input.DepartmentId);

		entity.Update(input.Number, input.Name, input.DepartmentId, input.IsLeader, input.Description);

		await _positionRepository.UpdateAsync(entity, autoSave: true);
		return ObjectMapper.Map<Position, PositionDto>(entity);
	}

	[HttpDelete("delete/{id}")]
	[Authorize(PmsPermissions.BaseData.Positions.Delete)]
	public async Task DeleteAsync(Guid id)
	{
		var entity = await _positionRepository.GetAsync(id);
		entity.EnsureDeletable();
		await _positionRepository.DeleteAsync(entity);
	}

	[HttpPost("submit/{id}")]
	[Authorize(PmsPermissions.BaseData.Positions.Submit)]
	public async Task SubmitAsync(Guid id)
	{
		var entity = await _positionRepository.GetAsync(id);
		entity.Submit();
		await _positionRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("cancel/{id}")]
	[Authorize(PmsPermissions.BaseData.Positions.Cancel)]
	public async Task CancelAsync(Guid id)
	{
		var entity = await _positionRepository.GetAsync(id);
		entity.UnAudit();
		await _positionRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("audit/{id}")]
	[Authorize(PmsPermissions.BaseData.Positions.Audit)]
	public async Task AuditAsync(Guid id)
	{
		var entity = await _positionRepository.GetAsync(id);
		entity.Audit(CurrentUser.Id!.Value, Clock.Now);
		await _positionRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("unaudit/{id}")]
	[Authorize(PmsPermissions.BaseData.Positions.UnAudit)]
	public async Task UnAuditAsync(Guid id)
	{
		var entity = await _positionRepository.GetAsync(id);
		entity.UnAudit();
		await _positionRepository.UpdateAsync(entity, autoSave: true);
	}
}
