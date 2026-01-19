using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using XD.Pms.ApiResponse;

namespace XD.Pms.Filters;

/// <summary>
/// API 响应包装过滤器
/// </summary>
public class ApiResponseWrapperFilter : IAsyncResultFilter
{
	public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
	{
		// 只处理 API 请求
		if (!IsApiRequest(context))
		{
			await next();
			return;
		}

		if (context.Result is ObjectResult objectResult)
		{
			// 如果已经是 ApiResponse 类型，不再包装
			if (objectResult.Value is IApiResponse)
			{
				await next();
				return;
			}

			// 包装响应
			var wrappedResult = new ApiResponse<object>(objectResult.StatusCode.ToString() ?? "200", true, objectResult.Value);
			context.Result = new ObjectResult(wrappedResult)
			{
				StatusCode = 200
			};
		}
		else if (context.Result is EmptyResult)
		{
			context.Result = new ObjectResult(ApiResponse<object>.Succeed(true, null))
			{
				StatusCode = 200
			};
		}

		await next();
	}

	private static bool IsApiRequest(ResultExecutingContext context)
	{
		var path = context.HttpContext.Request.Path.Value?.ToLower() ?? "";
		return path.StartsWith("/api/") && !path.StartsWith("/api/app/") && !path.StartsWith("/api/identity/");
	}
}