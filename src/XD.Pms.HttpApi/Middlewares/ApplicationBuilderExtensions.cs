using Microsoft.AspNetCore.Builder;
using System;

namespace XD.Pms.Middlewares;

public static class ApplicationBuilderExtensions
{
	public static IApplicationBuilder UseApiExceptionHandler(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<ApiExceptionHandlerMiddleware>();
	}
}
