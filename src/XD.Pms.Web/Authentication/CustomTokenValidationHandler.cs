using Microsoft.AspNetCore;
using Microsoft.Extensions.DependencyInjection;
using OpenIddict.Abstractions;
using OpenIddict.Validation;
using System;
using System.Linq;
using System.Threading.Tasks;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;

namespace XD.Pms.Web.Authentication;

public class CustomTokenValidationHandler : IOpenIddictValidationHandler<OpenIddictValidationEvents.ProcessAuthenticationContext>
{
	public static OpenIddictValidationHandlerDescriptor Descriptor { get; } =
		OpenIddictValidationHandlerDescriptor.CreateBuilder<OpenIddictValidationEvents.ProcessAuthenticationContext>()
			.UseSingletonHandler<CustomTokenValidationHandler>()
			// 放在 ValidateAccessToken 之后，黑名单之前
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
		var exp = context.AccessTokenPrincipal.GetExpirationDate(); // DateTimeOffset?

		// 1. AccessToken 已过期（最常见）
		if (exp.HasValue && exp.Value < DateTimeOffset.UtcNow)
		{
			// 关键：直接 Reject 并抛自定义异常，让 ApiExceptionFilter 捕获
			throw new AuthenticationException(
				ApiResponseCode.AccessTokenExpired,
				"Access token has expired.");
		}

		// 2. Token 被撤销（黑名单）
		if (!string.IsNullOrEmpty(jti))
		{
			var blacklistService = context.Transaction.GetHttpRequest()?.HttpContext
				.RequestServices.GetRequiredService<ITokenBlacklistService>();

			if (blacklistService != null && blacklistService.IsBlacklistedAsync(jti).GetAwaiter().GetResult())
			{
				// 可区分是被管理员踢了，还是单纯撤销
				throw new AuthenticationException(
					ApiResponseCode.AccessTokenRevoked,
					"Token has been revoked by administrator.");
			}
		}
	}
}