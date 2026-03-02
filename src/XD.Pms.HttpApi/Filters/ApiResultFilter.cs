using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Threading.Tasks;
using XD.Pms.ApiResponse;

namespace XD.Pms.Filters;

/// <summary>
/// API 响应包装结果过滤器
/// </summary>
public class ApiResultFilter : IAsyncResultFilter
{
	public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
	{
		if (!IsApiRequest(context))
		{
			await next();
			return;
		}

		if (context.Result is ObjectResult objectResult)
		{
			if (objectResult.Value is IApiResponse)
			{
				await next();
				return;
			}

			var wrappedResult = new ApiResponse<object>(objectResult.StatusCode?.ToString() ?? "200", true, objectResult.Value);
			context.Result = new ObjectResult(wrappedResult)
			{
				StatusCode = StatusCodes.Status200OK
			};
		}
		else if (context.Result is EmptyResult)
		{
			context.Result = new ObjectResult(ApiResponse<object>.Succeed(true, null))
			{
				StatusCode = StatusCodes.Status200OK
			};
		}

		await next();
	}

	private static bool IsApiRequest(ResultExecutingContext context)
	{
		var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";
		return path.StartsWith("/papi/") || path.StartsWith("/api/");
	}
}