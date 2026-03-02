using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using XD.Pms.BaseData.Positions;
using XD.Pms.Enums;

namespace XD.Pms.EntityFrameworkCore.Repositories;

public class EfCorePositionRepository : EfCoreRepository<PmsDbContext, Position, Guid>, IPositionRepository
{
	public EfCorePositionRepository(IDbContextProvider<PmsDbContext> dbContextProvider)
		: base(dbContextProvider) 
	{ }

	public async Task<Position?> FindByNumberAsync(string number)
	{
		return await (await GetDbSetAsync()).FirstOrDefaultAsync(p => p.Number == number);
	}

	public async Task<List<Position>> GetAllAsync()
	{
		return await (await GetDbSetAsync()).Include(p => p.Department).OrderBy(p => p.Number).ToListAsync();
	}

	public async Task<List<Position>> GetListAsync(
		string? number, string? name, Guid? departmentId, DocumentStatus? documentStatus,
		string? sorts, 
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync())
			.Include(p => p.Department)
			.WhereIf(!number.IsNullOrWhiteSpace(), p => p.Number.ToLower().Contains(number!.ToLower()))
			.WhereIf(!name.IsNullOrWhiteSpace(), p => p.Name.Contains(name!))
			.WhereIf(departmentId.HasValue, p => p.DepartmentId == departmentId)
			.WhereIf(documentStatus.HasValue, p => p.DocumentStatus == documentStatus)
			.OrderBy(sorts.IsNullOrWhiteSpace() ? $"{nameof(Position.Number)} asc" : sorts)
			.PageBy(skipCount, maxResultCount)
			.ToListAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<long> GetCountAsync(
		string? number, string? name, Guid? departmentId, DocumentStatus? documentStatus, CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync())
			.WhereIf(!number.IsNullOrWhiteSpace(), p => p.Number.Contains(number!))
			.WhereIf(!name.IsNullOrWhiteSpace(), p => p.Name.Contains(name!))
			.WhereIf(departmentId.HasValue, p => p.DepartmentId == departmentId)
			.WhereIf(documentStatus.HasValue, p => p.DocumentStatus == documentStatus)
			.LongCountAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<bool> AnyByDepartmentAsync(Guid departmentId)
	{
		return await (await GetDbSetAsync()).AnyAsync(p => p.DepartmentId == departmentId);
	}
}