using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using XD.Pms.Authentication;
using XD.Pms.Localization;

namespace XD.Pms.Web.Authentication;

/// <summary>
/// Token 验证事件处理器
/// </summary>
public class TokenBlacklistValidationHandler : IOpenIddictValidationHandler<OpenIddictValidationEvents.ProcessAuthenticationContext>
{
	public static OpenIddictValidationHandlerDescriptor Descriptor { get; }
		= OpenIddictValidationHandlerDescriptor.CreateBuilder<OpenIddictValidationEvents.ProcessAuthenticationContext>()
			.UseSingletonHandler<TokenBlacklistValidationHandler>()
			.SetOrder(OpenIddictValidationHandlers.Protection.ValidatePrincipal.Descriptor.Order + 1)
			.SetType(OpenIddictValidationHandlerType.Custom)
			.Build();

	public async ValueTask HandleAsync(OpenIddictValidationEvents.ProcessAuthenticationContext context)
	{
		string? accessToken = context.AccessToken;
		if (accessToken == null)
		{
			return;
		}

		var handler = new JwtSecurityTokenHandler();
		var jwtToken = handler.ReadJwtToken(accessToken);

		// 获取 JTI claim
		var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
		if (string.IsNullOrEmpty(jti))
		{
			return;
		}

		// 检查黑名单
		var blacklistService = context.Transaction.GetHttpRequest()?.HttpContext
			.RequestServices.GetService<ITokenBlacklistService>();

		if (blacklistService != null && await blacklistService.IsBlacklistedAsync(jti))
		{
			var localizer = context.Transaction.GetHttpRequest()?.HttpContext
				.RequestServices.GetService<IStringLocalizer<PmsResource>>();
			context.Reject(
				error: OpenIddictConstants.Errors.InvalidToken,
				description: localizer?["Auth:TokenRevoked"]?.Value ?? "The token has been revoked."
			);
		}
	}
}