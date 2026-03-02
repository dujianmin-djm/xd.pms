using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using XD.Pms.Enums;

namespace XD.Pms.BaseData.Employees;

public interface IEmployeeRepository : IRepository<Employee, Guid>
{
	Task<Employee?> FindByNumberAsync(string number);
	Task<Employee?> GetWithDetailsAsync(Guid id);
	Task<List<Employee>> GetListAsync(
		string? number, 
		string? name, 
		Gender? gender, 
		string? phone,
		DocumentStatus? documentStatus,
		string? sorting = null,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default);
	Task<long> GetCountAsync(
		string? number, 
		string? name, 
		Gender? gender, 
		string? phone,
		DocumentStatus? documentStatus,
		CancellationToken cancellationToken = default);
}