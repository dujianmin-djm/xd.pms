using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using XD.Pms.Identity;

namespace XD.Pms.EntityFrameworkCore.Repositories;

public class EfCoreRoleRepository : EfCoreRepository<PmsDbContext, IdentityRole, Guid>, IRoleRepository
{
	public EfCoreRoleRepository(IDbContextProvider<PmsDbContext> dbContextProvider) : base(dbContextProvider)
	{

	}

	public async Task<List<IdentityRole>> GetListAsync(
		string? number = null, 
		string? name = null, 
		bool? isActive = null,
		string? sorting = null, 
		int skipCount = 0, 
		int maxResultCount = int.MaxValue, 
		bool includeDetails = false, 
		CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync().ConfigureAwait(false))
			.IncludeDetails(includeDetails)
			.WhereIf(!number.IsNullOrWhiteSpace(), x => EF.Functions.Like(EF.Property<string>(x, "Number"), $"%{number}%"))
			.WhereIf(!name.IsNullOrWhiteSpace(), x => x.Name.Contains(name!) || x.NormalizedName.Contains(name!))
			.WhereIf(isActive.HasValue, x => EF.Property<bool>(x, "IsActive") == isActive!.Value)
			.OrderBy(sorting.IsNullOrWhiteSpace() ? $"{nameof(IdentityRole.CreationTime)} desc" : sorting)
			.PageBy(skipCount, maxResultCount)
			.ToListAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<List<IdentityRole>> GetListAsync(
		string? filter = null, 
		string? sorting = null, 
		int skipCount = 0, 
		int maxResultCount = int.MaxValue, 
		bool includeDetails = false, 
		CancellationToken cancellationToken = default)
	{
		return await(await GetDbSetAsync().ConfigureAwait(false))
			.IncludeDetails(includeDetails)
			.WhereIf(!filter.IsNullOrWhiteSpace(),
				x => x.Name.Contains(filter!) || x.NormalizedName.Contains(filter!)
					|| EF.Functions.Like(EF.Property<string>(x, "Number"), $"%{filter}%"))
			.OrderBy(sorting.IsNullOrWhiteSpace() ? $"{nameof(IdentityRole.CreationTime)} desc" : sorting)
			.PageBy(skipCount, maxResultCount)
			.ToListAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync().ConfigureAwait(false))
			.WhereIf(!filter.IsNullOrWhiteSpace(),
				x => x.Name.Contains(filter!) || x.NormalizedName.Contains(filter!)
					|| EF.Functions.Like(EF.Property<string>(x, "Number"), $"%{filter}%"))
			.LongCountAsync(GetCancellationToken(cancellationToken)).ConfigureAwait(false);
	}

	public async Task<long> GetCountAsync(string? number = null, string? name = null, bool? isActive = null, CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync().ConfigureAwait(false))
			.WhereIf(!number.IsNullOrWhiteSpace(), x => EF.Functions.Like(EF.Property<string>(x, "Number"), $"%{number}%"))
			.WhereIf(!name.IsNullOrWhiteSpace(), x => x.Name.Contains(name!) || x.NormalizedName.Contains(name!))
			.WhereIf(isActive.HasValue, x => EF.Property<bool>(x, "IsActive") == isActive!.Value)
			.LongCountAsync(GetCancellationToken(cancellationToken)).ConfigureAwait(false);
	}

	public async Task<bool> CheckNumberExistsAsync(string number, Guid? ignoreId = null, CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync().ConfigureAwait(false))
			.Where(x => EF.Property<string>(x, "Number").ToLower() == number.ToLower())
			.WhereIf(ignoreId.HasValue, x => x.Id != ignoreId!.Value)
			.AnyAsync(GetCancellationToken(cancellationToken));
	}
}
