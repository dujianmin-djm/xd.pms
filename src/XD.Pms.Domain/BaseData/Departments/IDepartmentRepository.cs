using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using XD.Pms.Enums;

namespace XD.Pms.BaseData.Departments;

public interface IDepartmentRepository : IRepository<Department, Guid>
{
	Task<Department?> FindByNumberAsync(string number);
	Task<List<Department>> GetAllAsync();
	Task<List<Department>> GetListAsync(
		string? number, 
		string? name, 
		DocumentStatus? documentStatus,
		string? sorting = null,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default);
	Task<long> GetCountAsync(
		string? number, 
		string? name, 
		DocumentStatus? documentStatus, 
		CancellationToken cancellationToken = default);
	Task<bool> HasChildrenAsync(Guid id);
	Task<List<Department>> GetDescendantsAsync(Guid parentId);
}