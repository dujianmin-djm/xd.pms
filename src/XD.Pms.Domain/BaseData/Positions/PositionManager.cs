using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Services;
using XD.Pms.BaseData.Departments;

namespace XD.Pms.BaseData.Positions;

public class PositionManager : DomainService
{
	private readonly IPositionRepository _positionRepository;
	private readonly IDepartmentRepository _departmentRepository;

	public PositionManager(IPositionRepository positionRepository, IDepartmentRepository departmentRepository)
	{
		_positionRepository = positionRepository;
		_departmentRepository = departmentRepository;
	}

	public async Task CheckNumberDuplicateAsync(string number, Guid? excludeId = null)
	{
		var existing = await _positionRepository.FindByNumberAsync(number);
		if (existing != null && existing.Id != excludeId)
			throw new BusinessException(PmsDomainErrorCodes.DuplicateNumber).WithData("Number", number);
	}

	public async Task CheckDepartmentExistsAsync(Guid departmentId)
	{
		if (!await _departmentRepository.AnyAsync(d => d.Id == departmentId))
			throw new BusinessException(PmsDomainErrorCodes.PositionDepartmentNotFound);
	}
}