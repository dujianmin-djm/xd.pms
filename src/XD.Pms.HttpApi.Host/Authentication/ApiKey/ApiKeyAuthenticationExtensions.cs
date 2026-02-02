using Microsoft.AspNetCore.Authentication;
using System;

namespace XD.Pms.Authentication.ApiKey;

public static class ApiKeyAuthenticationExtensions
{
	public static AuthenticationBuilder AddApiKey(
		this AuthenticationBuilder builder,
		Action<ApiKeyAuthenticationOptions>? configureOptions = null)
	{
		return builder.AddApiKey(ApiKeyAuthenticationOptions.DefaultScheme, configureOptions);
	}

	public static AuthenticationBuilder AddApiKey(
		this AuthenticationBuilder builder,
		string authenticationScheme,
		Action<ApiKeyAuthenticationOptions>? configureOptions = null)
	{
		return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
			authenticationScheme,
			configureOptions);
	}
}