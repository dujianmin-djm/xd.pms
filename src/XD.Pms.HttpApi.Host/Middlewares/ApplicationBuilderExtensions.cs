using Microsoft.AspNetCore.Builder;
using System;

namespace XD.Pms.Middlewares;

public static class ApplicationBuilderExtensions
{
	/// <summary>
	/// Api响应处理中间件
	/// </summary>
	public static IApplicationBuilder UseApiResponseHandler(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<ApiResponseHandlerMiddleware>();
	}

	/// <summary>
	/// Api Token请求头转换中间件
	/// </summary>
	public static IApplicationBuilder UseTokenHeaderTransform(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<TokenHeaderTransformMiddleware>();
	}
}
