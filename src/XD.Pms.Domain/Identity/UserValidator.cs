using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;

namespace XD.Pms.Identity;

public class UserValidator : AbpIdentityUserValidator, ITransientDependency
{
	public UserValidator(
		IOptions<AbpMultiTenancyOptions> multiTenancyOptions, 
		IAbpDistributedLock distributedLock, 
		ICurrentTenant currentTenant, 
		IDataFilter<IMultiTenant> tenantFilter, 
		IIdentityUserRepository userRepository) 
		: base(multiTenancyOptions, distributedLock, currentTenant, tenantFilter, userRepository)
	{
	}

	public override async Task<IdentityResult> ValidateAsync(UserManager<IdentityUser> manager, IdentityUser user)
	{
		var result = await base.ValidateAsync(manager, user);
		if (!result.Succeeded)
		{
			result.Errors.ToList().RemoveAll(e => e.Code == "InvalidUserName");
		}
		return result;
	}
}
