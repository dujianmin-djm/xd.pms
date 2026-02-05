using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using XD.Pms.ApiKeys;

namespace XD.Pms.EntityFrameworkCore.Repositories;

public class EfCoreApiKeyRepository : EfCoreRepository<PmsDbContext, ApiKey, Guid>, IApiKeyRepository
{
	public EfCoreApiKeyRepository(IDbContextProvider<PmsDbContext> dbContextProvider)
		: base(dbContextProvider)
	{
	}

	public async Task<ApiKey?> FindByKeyHashAsync(string keyHash, CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();

		return await dbSet
			.FirstOrDefaultAsync(x => x.KeyHash == keyHash, cancellationToken);
	}

	public async Task<ApiKey?> FindByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();

		return await dbSet
			.FirstOrDefaultAsync(x => x.ClientId == clientId, cancellationToken);
	}

	public async Task<bool> ClientIdExistsAsync(string clientId, Guid? excludeId = null, CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();

		return await dbSet
			.Where(x => x.ClientId == clientId)
			.WhereIf(excludeId.HasValue, x => x.Id != excludeId!.Value)
			.AnyAsync(cancellationToken);
	}

	public async Task<List<ApiKey>> GetListAsync(
		string? filter = null,
		bool? isActive = null,
		Guid? userId = null,
		string? sorting = null,
		int maxResultCount = int.MaxValue,
		int skipCount = 0,
		CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();

		return await dbSet
			.WhereIf(!string.IsNullOrWhiteSpace(filter), x =>
				x.ClientId.Contains(filter!) ||
				x.ClientName.Contains(filter!) ||
				(x.Description != null && x.Description.Contains(filter!)))
			.WhereIf(isActive.HasValue, x => x.IsActive == isActive!.Value)
			.WhereIf(userId.HasValue, x => x.UserId == userId!.Value)
			.OrderBy(sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : sorting)
			.PageBy(skipCount, maxResultCount)
			.ToListAsync(cancellationToken);
	}

	public async Task<long> GetCountAsync(
		string? filter = null,
		bool? isActive = null,
		Guid? userId = null,
		CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();

		return await dbSet
			.WhereIf(!string.IsNullOrWhiteSpace(filter), x =>
				x.ClientId.Contains(filter!) ||
				x.ClientName.Contains(filter!) ||
				(x.Description != null && x.Description.Contains(filter!)))
			.WhereIf(isActive.HasValue, x => x.IsActive == isActive!.Value)
			.WhereIf(userId.HasValue, x => x.UserId == userId!.Value)
			.LongCountAsync(cancellationToken);
	}

	public async Task UpdateLastUsedAsync(
		Guid id,
		DateTime lastUsedAt,
		string? lastUsedIp,
		CancellationToken cancellationToken = default)
	{
		var dbContext = await GetDbContextAsync();

		await dbContext.Database.ExecuteSqlInterpolatedAsync(
			$@"UPDATE T_SYS_ApiKeys 
               SET LastUsedAt = {lastUsedAt}, 
                   LastUsedIp = {lastUsedIp},
                   UsageCount = UsageCount + 1
               WHERE Id = {id}",
			cancellationToken);
	}
}