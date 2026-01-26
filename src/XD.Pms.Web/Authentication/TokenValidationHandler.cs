using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
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
		try
		{
			var handler = new JwtSecurityTokenHandler();
			if (!handler.CanReadToken(accessToken))
			{
				return;
			}
			var jwtToken = handler.ReadJwtToken(accessToken);
			var jti = jwtToken.Claims.FirstOrDefault(c => c.Type == "jti")?.Value;
			if (string.IsNullOrEmpty(jti))
			{
				return;
			}

			var httpContext = context.Transaction.GetHttpRequest()?.HttpContext;
			if (httpContext == null)
			{
				return;
			}
			// 检查 access token 是否在黑名单
			var blacklistService = httpContext.RequestServices.GetService<ITokenBlacklistService>();
			if (blacklistService != null && await blacklistService.IsBlacklistedAsync(jti))
			{
				var localizer = httpContext.RequestServices.GetService<IStringLocalizer<PmsResource>>();
				context.Reject(
					error: "access_token_revoked",
					description: localizer?["Auth:AccessTokenRevoked"]?.Value ?? "The access token has been revoked."
				);
			}
		}
		catch
		{
			// 忽略解析错误，让后续处理程序处理
		}
	}
}