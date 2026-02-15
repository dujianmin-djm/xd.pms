using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenIddict.Validation;
using System.Linq;
using System.Threading.Tasks;
using XD.Pms.ApiResponse;
using XD.Pms.Localization;

namespace XD.Pms.Authentication;

public class TokenValidationHandler : IOpenIddictValidationHandler<OpenIddictValidationEvents.ProcessAuthenticationContext>
{
	public static OpenIddictValidationHandlerDescriptor Descriptor { get; } =
		OpenIddictValidationHandlerDescriptor.CreateBuilder<OpenIddictValidationEvents.ProcessAuthenticationContext>()
			.UseSingletonHandler<TokenValidationHandler>()
			// 放在 ValidateAccessToken 之后
			.SetOrder(OpenIddictValidationHandlers.ValidateAccessToken.Descriptor.Order + 10)
			.SetType(OpenIddictValidationHandlerType.Custom)
			.Build();

	public async ValueTask HandleAsync(OpenIddictValidationEvents.ProcessAuthenticationContext context)
	{
		if (context.AccessTokenPrincipal == null)
		{
			return;
		}
		var claims = context.AccessTokenPrincipal.Claims;
		var jti = claims.FirstOrDefault(c => c.Type == "jti")?.Value;
		if (!string.IsNullOrEmpty(jti))
		{
			var httpContext = context.Transaction.GetHttpRequest()?.HttpContext;
			if (httpContext == null)
			{
				return;
			}
			var blacklistService = httpContext.RequestServices.GetRequiredService<ITokenBlacklistService>();
			if (blacklistService != null && await blacklistService.IsBlacklistedAsync(jti))
			{
				var localizer = httpContext.RequestServices.GetRequiredService<IStringLocalizer<PmsResource>>();

				throw new PmsBusinessException(
					ApiResponseCode.AccessTokenRevoked,
					localizer?["Auth:AccessTokenRevoked"]?.Value ?? "The access token has been revoked."
				);
			}
		}
	}
}