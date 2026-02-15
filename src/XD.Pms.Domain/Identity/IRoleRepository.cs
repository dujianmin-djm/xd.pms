using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;

namespace XD.Pms.Identity;

public interface IRoleRepository : IRepository<IdentityRole, Guid>
{
	Task<List<IdentityRole>> GetListAsync(
		string? number = null,
		string? name = null,
		bool? isActive = null,
		string? sorting = null,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		bool includeDetails = false,
		CancellationToken cancellationToken = default);

	Task<List<IdentityRole>> GetListAsync(
		string? filter = null,
		string? sorting = null,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		bool includeDetails = false,
		CancellationToken cancellationToken = default);

	Task<long> GetCountAsync(string? number = null, string? name = null, bool? isActive = null, CancellationToken cancellationToken = default);
	Task<long> GetCountAsync(string? filter = null, CancellationToken cancellationToken = default);
	Task<bool> CheckNumberExistsAsync(string number, Guid? ignoreId = null, CancellationToken cancellationToken = default);
}
