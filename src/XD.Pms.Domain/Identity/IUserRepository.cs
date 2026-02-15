using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Identity;
using XD.Pms.Enums;

namespace XD.Pms.Identity;

public interface IUserRepository
{
	Task<List<IdentityUser>> GetListAsync(
		string? userName = null,
		Gender? gender = null,
		string? phoneNumber = null,
		string? email = null,
		bool? isActive = null,
		string? sorting = null,
		int skipCount = 0,
		int maxResultCount = int.MaxValue,
		CancellationToken cancellationToken = default);

	Task<long> GetCountAsync(
		string? userName = null,
		Gender? gender = null,
		string? phoneNumber = null,
		string? email = null,
		bool? isActive = null, 
		CancellationToken cancellationToken = default);
}
