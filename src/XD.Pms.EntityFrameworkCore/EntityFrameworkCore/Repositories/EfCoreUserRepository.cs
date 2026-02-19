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
using XD.Pms.Enums;
using XD.Pms.Identity;

namespace XD.Pms.EntityFrameworkCore.Repositories;

public class EfCoreUserRepository : EfCoreRepository<PmsDbContext, IdentityUser, Guid>, IUserRepository
{
	public EfCoreUserRepository(IDbContextProvider<PmsDbContext> dbContextProvider) : base(dbContextProvider)
	{

	}

	public async Task<long> GetCountAsync(
		string? userName = null,
		Gender? gender = null,
		string? phoneNumber = null,
		string? email = null,
		bool? isActive = null, 
		CancellationToken cancellationToken = default)
	{
		return await (await GetDbSetAsync().ConfigureAwait(false))
			.WhereIf(!userName.IsNullOrWhiteSpace(), x => x.UserName.Contains(userName!) || x.NormalizedUserName.Contains(userName!))
			.WhereIf(!phoneNumber.IsNullOrWhiteSpace(), x => x.PhoneNumber.Contains(phoneNumber!))
			.WhereIf(!email.IsNullOrWhiteSpace(), x => x.Email.Contains(email!) || x.NormalizedEmail.Contains(email!))
			.WhereIf(gender.HasValue, x => EF.Property<Gender>(x, "Gender") == gender!.Value)
			.WhereIf(isActive.HasValue, x => x.IsActive == isActive!.Value)
			.LongCountAsync(GetCancellationToken(cancellationToken));
	}

	public async Task<List<IdentityUser>> GetListAsync(
		string? userName = null, 
		Gender? gender = null, 
		string? phoneNumber = null, 
		string? email = null, 
		bool? isActive = null, 
		string? sorting = null, 
		int skipCount = 0, 
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default)
	{
		return await(await GetDbSetAsync().ConfigureAwait(false))
			//.Include(u => u.Roles)
			.WhereIf(!userName.IsNullOrWhiteSpace(), x => x.UserName.Contains(userName!) || x.NormalizedUserName.Contains(userName!))
			.WhereIf(!phoneNumber.IsNullOrWhiteSpace(), x => x.PhoneNumber.Contains(phoneNumber!))
			.WhereIf(!email.IsNullOrWhiteSpace(), x => x.Email.Contains(email!) || x.NormalizedEmail.Contains(email!))
			.WhereIf(gender.HasValue, x => EF.Property<Gender>(x, "Gender") == gender!.Value)
			.WhereIf(isActive.HasValue, x => x.IsActive == isActive!.Value)
			.OrderBy(sorting.IsNullOrWhiteSpace() ? $"{nameof(IdentityUser.CreationTime)} desc" : sorting)
			.PageBy(skipCount, maxResultCount)
			.ToListAsync(GetCancellationToken(cancellationToken));
	}
}
