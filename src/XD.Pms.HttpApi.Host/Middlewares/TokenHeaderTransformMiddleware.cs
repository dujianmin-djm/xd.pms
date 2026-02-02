using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using System.Threading.Tasks;

namespace XD.Pms.Middlewares;

/// <summary>
/// Token 请求头转换中间件
/// 将自定义请求头中的 Token 转换为标准的 Authorization Bearer 格式
/// </summary>
public class TokenHeaderTransformMiddleware(RequestDelegate next)
{
	private readonly RequestDelegate _next = next;

	/// <summary>
	/// 支持的自定义 Token 请求头名称（不区分大小写）
	/// </summary>
	private static readonly string[] CustomTokenHeaders =
	[
		"ApiAccessToken",
		"X-Access-Token"
	];

	public async Task InvokeAsync(HttpContext context)
	{
		if (context.Request.Headers.ContainsKey("Authorization"))
		{
			await _next(context);
			return;
		}

		foreach (var headerName in CustomTokenHeaders)
		{
			if (context.Request.Headers.TryGetValue(headerName, out StringValues tokenValue)
				&& !string.IsNullOrWhiteSpace(tokenValue))
			{
				var token = tokenValue.ToString().Trim();
				var authorizationValue = token.StartsWith("Bearer ", System.StringComparison.OrdinalIgnoreCase)
					? token
					: $"Bearer {token}";

				context.Request.Headers.Authorization = authorizationValue;
				break;
			}
		}

		await _next(context);
	}
}