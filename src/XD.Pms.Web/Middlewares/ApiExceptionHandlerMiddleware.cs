using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Validation;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;
using XD.Pms.Localization;

namespace XD.Pms.Web.Middlewares;

/// <summary>
/// API 异常处理中间件
/// </summary>
public class ApiExceptionHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ApiExceptionHandlerMiddleware> _logger;

	private static readonly JsonSerializerOptions JsonOptions = new()
	{
		PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
		WriteIndented = false
	};

	public ApiExceptionHandlerMiddleware(RequestDelegate next, ILogger<ApiExceptionHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context, IStringLocalizer<PmsResource> localizer)
	{
		try
		{
			await _next(context);

			// 处理 401/403 等状态码（未被异常捕获的情况）
			if (!context.Response.HasStarted && IsApiRequest(context))
			{
				if (context.Response.StatusCode >= 400 && context.Response.ContentLength == null)
				{
					await HandleStatusCodeAsync(context, localizer);
				}
			}
		}
		catch (Exception ex)
		{
			if (IsApiRequest(context) && !context.Response.HasStarted)
			{
				if (context.Response.StatusCode >= 400 && context.Response.ContentLength == null)
				{
					await HandleStatusCodeAsync(context, localizer);
					return;
				}
				await HandleExceptionAsync(context, ex, localizer);
			}
			else
			{
				throw;
			}
		}
	}

	private static async Task HandleStatusCodeAsync(HttpContext context, IStringLocalizer<PmsResource> localizer)
	{
		var statusCode = context.Response.StatusCode;
		var (code, messageKey) = statusCode switch
		{
			400 => (ApiResponseCode.BadRequest, "Auth:BadRequest"),
			401 => (ApiResponseCode.Unauthorized, "Auth:Unauthorized"),
			403 => (ApiResponseCode.Forbidden, "Auth:Forbidden"),
			404 => (ApiResponseCode.NotFound, "Auth:NotFound"),
			405 => (ApiResponseCode.MethodNotAllowed, "Auth:MethodNotAllowed"),
			429 => (ApiResponseCode.TooManyRequests, "Auth:TooManyRequests"),
			_ => (ApiResponseCode.InternalError, "Auth:ServerError")
		};
		await WriteResponseAsync(context, code, localizer[messageKey].Value);
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception, IStringLocalizer<PmsResource> localizer)
	{
		_logger.LogError(exception, "API 请求异常: {Path}", context.Request.Path);

		var (code, message) = exception switch
		{
			// 自定义认证异常
			AuthenticationException authEx
				=> (authEx.ErrorCode, authEx.Message),

			// ABP 授权异常
			AbpAuthorizationException
				=> (ApiResponseCode.Forbidden, localizer["Auth:Forbidden"].Value),

			// ABP 验证异常
			AbpValidationException validationEx
				=> (ApiResponseCode.ValidationError, FormatValidationErrors(validationEx)),

			// 实体不存在
			EntityNotFoundException
				=> (ApiResponseCode.NotFound, localizer["Auth:NotFound"].Value),

			// ABP 业务异常
			UserFriendlyException userEx
				=> (ApiResponseCode.BadRequest, userEx.Message),

			// ABP 业务异常
			BusinessException bizEx
				=> (ApiResponseCode.BadRequest, bizEx.Message),

			// 操作取消
			OperationCanceledException ocEx
				=> (ApiResponseCode.BadRequest, ocEx.Message),

			// 参数异常
			ArgumentException argEx
				=> (ApiResponseCode.ValidationError, argEx.Message),

			// 未处理的异常
			_ => (ApiResponseCode.InternalError, localizer["Auth:ServerError"].Value)
		};

		await WriteResponseAsync(context, code, message);
	}

	private static async Task WriteResponseAsync(HttpContext context, string code, string message)
	{
		context.Response.StatusCode = 200;
		context.Response.ContentType = "application/json; charset=utf-8";

		var response = ApiResponse<object>.Fail(code, message);

		await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
	}

	private static bool IsApiRequest(HttpContext context)
	{
		var path = context.Request.Path.Value?.ToLower() ?? "";
		return path.StartsWith("/papi/");
	}

	private static string FormatValidationErrors(AbpValidationException ex)
	{
		var errors = ex.ValidationErrors
			.SelectMany(e => e.MemberNames.Select(m => $"{m}: {e.ErrorMessage}"))
			.ToList();

		return string.Join("; ", errors);
	}
}