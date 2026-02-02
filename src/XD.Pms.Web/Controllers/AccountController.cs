using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.Authentication;

namespace XD.Pms.Web.Controllers;

[Route("Account/[action]")]
public class AccountController : ChallengeAccountController
{
	public AccountController() : base(["oidc"])
	{

	}

	public override async Task<ActionResult> LoginAsync(string returnUrl = "", string returnUrlHash = "")
	{
		if (CurrentUser.IsAuthenticated)
		{
			return await RedirectSafelyAsync(returnUrl, returnUrlHash);
		}

		return Challenge(new AuthenticationProperties { RedirectUri = await GetRedirectUrlAsync(returnUrl, returnUrlHash) }, ChallengeAuthenticationSchemas);
	}
}