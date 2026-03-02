using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using XD.Pms.BaseData.Departments;
using XD.Pms.Enums;

namespace XD.Pms.EntityFrameworkCore.Repositories;

public class EfCoreDepartmentRepository : EfCoreRepository<PmsDbContext, Department, Guid>, IDepartmentRepository
{
	public EfCoreDepartmentRepository(IDbContextProvider<PmsDbContext> dbContextProvider)
		: base(dbContextProvider) 
	{ }

	public async Task<Department?> FindByNumberAsync(string number)
	{
		return await (await GetDbSetAsync()).FirstOrDefaultAsync(d => d.Number == number);
	}

	public async Task<List<Department>> GetAllAsync()
	{
		return await (await GetDbSetAsync()).OrderBy(d => d.Number).ToListAsync();
	}

	public async Task<List<Department>> GetListAsync(
		string? number, string? name, DocumentStatus? documentStatus,
		string? sorting = null,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync().ConfigureAwait(false))
			.Include(d => d.Parent)
			.WhereIf(!number.IsNullOrWhiteSpace(), d => d.Number.ToLower().Contains(number!.ToLower()))
			.WhereIf(!name.IsNullOrWhiteSpace(), d => d.Name.Contains(name!))
			.WhereIf(documentStatus.HasValue, d => d.DocumentStatus == documentStatus)
			.OrderBy(sorting.IsNullOrWhiteSpace() ? $"{nameof(Department.Number)} asc" : sorting)
			.PageBy(skipCount, maxResultCount)
			.ToListAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<long> GetCountAsync(string? number, string? name, DocumentStatus? documentStatus, CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync())
			.WhereIf(!number.IsNullOrWhiteSpace(), d => d.Number.ToLower().Contains(number!.ToLower()))
			.WhereIf(!name.IsNullOrWhiteSpace(), d => d.Name.Contains(name!))
			.WhereIf(documentStatus.HasValue, d => d.DocumentStatus == documentStatus)
			.LongCountAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<bool> HasChildrenAsync(Guid id)
	{
		return await (await GetDbSetAsync()).AnyAsync(d => d.ParentId == id);
	}

	public async Task<List<Department>> GetDescendantsAsync(Guid parentId)
	{
		var all = await (await GetDbSetAsync()).ToListAsync();
		var result = new List<Department>();
		CollectDescendants(all, parentId, result);
		return result;
	}

	private void CollectDescendants(List<Department> all, Guid parentId, List<Department> result)
	{
		var children = all.Where(d => d.ParentId == parentId).ToList();
		foreach (var child in children)
		{
			result.Add(child);
			CollectDescendants(all, child.Id, result);
		}
	}
}