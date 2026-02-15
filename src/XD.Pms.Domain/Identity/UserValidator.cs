using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Localization;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.Identity.Localization;

namespace XD.Pms.Identity;

public class UserValidator : AbpIdentityUserValidator, ITransientDependency
{
	public UserValidator(IStringLocalizer<IdentityResource> localizer) : base(localizer)
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
