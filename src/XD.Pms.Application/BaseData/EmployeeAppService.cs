using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using XD.Pms.BaseData.Employees;
using XD.Pms.BaseData.Employees.Dto;
using XD.Pms.Permissions;
using XD.Pms.Services.Dtos;

namespace XD.Pms.BaseData;

[Route("papi/employee")]
[Authorize(PmsPermissions.BaseData.Employees.Default)]
public class EmployeeAppService : PmsAppService, IEmployeeAppService
{
	private readonly IEmployeeRepository _employeeRepository;
	private readonly EmployeeManager _employeeManager;

	public EmployeeAppService(IEmployeeRepository employeeRepository, EmployeeManager employeeManager)
	{
		_employeeRepository = employeeRepository;
		_employeeManager = employeeManager;
	}

	[HttpGet("{id}")]
	public async Task<EmployeeDto> GetAsync(Guid id)
	{
		var entity = await _employeeRepository.GetWithDetailsAsync(id)
					 ?? throw new EntityNotFoundException(typeof(Employee), id);
		return MapToDto(entity);
	}

	[HttpGet("query")]
	public async Task<PagedResponseDto<EmployeeDto>> GetListAsync(EmployeeReadDto input)
	{
		int skip = (input.Current - 1) * input.Size;
		var list = await _employeeRepository.GetListAsync(
			input.Number, input.Name, input.Gender, input.Phone, input.DocumentStatus,
			input.Sorts, skip, input.Size);
		long total = await _employeeRepository.GetCountAsync(
			input.Number, input.Name, input.Gender, input.Phone, input.DocumentStatus);

		var dtos = list.Select(MapToDto).ToList();

		return new PagedResponseDto<EmployeeDto>(total, dtos)
		{
			Current = input.Current,
			Size = input.Size,
			Sorts = input.Sorts
		};
	}

	[HttpPost("add")]
	[Authorize(PmsPermissions.BaseData.Employees.Create)]
	public async Task<EmployeeDto> CreateAsync(EmployeeCreateDto input)
	{
		await _employeeManager.CheckNumberDuplicateAsync(input.Number);

		var entity = new Employee(
			GuidGenerator.Create(),
			input.Number, input.Name, input.Gender,
			input.HireDate, input.Phone, input.Email, input.Address, input.Description);

		entity.SyncPositions(
			input.Positions.Select(pa => 
				((Guid?)pa.Id, pa.DepartmentId, pa.PositionId, pa.StartDate, pa.IsPrimary)).ToList(),
			() => GuidGenerator.Create()
		);

		await _employeeRepository.InsertAsync(entity, autoSave: true);
		return MapToDto(entity);
	}

	[HttpPut("edit/{id}")]
	[Authorize(PmsPermissions.BaseData.Employees.Update)]
	public async Task<EmployeeDto> UpdateAsync(Guid id, EmployeeUpdateDto input)
	{
		var entity = await _employeeRepository.GetWithDetailsAsync(id)
					 ?? throw new EntityNotFoundException(typeof(Employee), id);
		entity.SetConcurrencyStampIfNotNull(input.ConcurrencyStamp);

		await _employeeManager.CheckNumberDuplicateAsync(input.Number, id);

		entity.Update(input.Number, input.Name, input.Gender,
					  input.HireDate, input.Phone, input.Email, input.Address, input.Description);

		entity.SyncPositions(
			input.Positions.Select(pa =>
				(pa.Id, pa.DepartmentId, pa.PositionId, pa.StartDate, pa.IsPrimary)).ToList(),
			() => GuidGenerator.Create()
		);

		await _employeeRepository.UpdateAsync(entity, autoSave: true);
		return MapToDto(entity);
	}

	[HttpDelete("delete/{id}")]
	[Authorize(PmsPermissions.BaseData.Employees.Delete)]
	public async Task DeleteAsync(Guid id)
	{
		var entity = await _employeeRepository.GetAsync(id);
		entity.EnsureDeletable();
		await _employeeRepository.DeleteAsync(entity);
	}

	[HttpPost("submit/{id}")]
	[Authorize(PmsPermissions.BaseData.Employees.Submit)]
	public async Task SubmitAsync(Guid id)
	{
		var entity = await _employeeRepository.GetAsync(id);
		entity.Submit();
		await _employeeRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("cancel/{id}")]
	[Authorize(PmsPermissions.BaseData.Employees.Cancel)]
	public async Task CancelAsync(Guid id)
	{
		var entity = await _employeeRepository.GetAsync(id);
		entity.UnAudit();
		await _employeeRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("audit/{id}")]
	[Authorize(PmsPermissions.BaseData.Employees.Audit)]
	public async Task AuditAsync(Guid id)
	{
		var entity = await _employeeRepository.GetAsync(id);
		entity.Audit(CurrentUser.Id!.Value, Clock.Now);
		await _employeeRepository.UpdateAsync(entity, autoSave: true);
	}

	[HttpPost("unaudit/{id}")]
	[Authorize(PmsPermissions.BaseData.Employees.UnAudit)]
	public async Task UnAuditAsync(Guid id)
	{
		var entity = await _employeeRepository.GetAsync(id);
		entity.UnAudit();
		await _employeeRepository.UpdateAsync(entity, autoSave: true);
	}

	/* ── 映射辅助 ── */
	private EmployeeDto MapToDto(Employee entity)
	{
		var dto = ObjectMapper.Map<Employee, EmployeeDto>(entity);
		dto.Positions = entity.Positions
			.Select(pa => new EmployeePositionDto
			{
				Id = pa.Id,
				DepartmentId = pa.DepartmentId,
				DepartmentName = pa.Department?.Name,
				PositionId = pa.PositionId,
				PositionName = pa.Position?.Name,
				StartDate = pa.StartDate,
				IsPrimary = pa.IsPrimary
			}).ToList();

		var primary = dto.Positions.FirstOrDefault(pa => pa.IsPrimary);
		dto.PrimaryDepartmentName = primary?.DepartmentName;
		dto.PrimaryPositionName = primary?.PositionName;
		return dto;
	}
}