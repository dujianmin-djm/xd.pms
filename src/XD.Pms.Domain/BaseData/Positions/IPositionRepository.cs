using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using XD.Pms.Enums;

namespace XD.Pms.BaseData.Positions;

public interface IPositionRepository : IRepository<Position, Guid>
{
	Task<Position?> FindByNumberAsync(string number);
	Task<List<Position>> GetAllAsync();
	Task<List<Position>> GetListAsync(
		string? number, 
		string? name, 
		Guid? departmentId, 
		DocumentStatus? documentStatus,
		string? sorting = null,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default);
	Task<long> GetCountAsync(
		string? number, 
		string? name, 
		Guid? departmentId, 
		DocumentStatus? documentStatus,
		CancellationToken cancellationToken = default);
	Task<bool> AnyByDepartmentAsync(Guid departmentId);
}