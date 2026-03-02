using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;

namespace XD.Pms.BaseData.Employees;

public class EmployeeManager : DomainService
{
	private readonly IEmployeeRepository _employeeRepository;

	public EmployeeManager(IEmployeeRepository employeeRepository)
	{
		_employeeRepository = employeeRepository;
	}

	public async Task CheckNumberDuplicateAsync(string number, Guid? excludeId = null)
	{
		var existing = await _employeeRepository.FindByNumberAsync(number);
		if (existing != null && existing.Id != excludeId)
			throw new BusinessException(PmsDomainErrorCodes.DuplicateNumber).WithData("Number", number);
	}
}
