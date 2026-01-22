using Microsoft.AspNetCore.Builder;
using System;

namespace XD.Pms.Web.Middlewares;

public static class ApplicationBuilderExtensions
{
	/// <summary>
	/// Api响应处理中间件
	/// </summary>
	public static IApplicationBuilder UseApiResponseHandler(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<ApiResponseHandlerMiddleware>();
	}

	// Api语言设置中间件
	public static IApplicationBuilder UseApiLanguageSetting(this IApplicationBuilder builder)
	{
		return builder.UseMiddleware<ApiLanguageSettingMiddleware>();
	}
}
