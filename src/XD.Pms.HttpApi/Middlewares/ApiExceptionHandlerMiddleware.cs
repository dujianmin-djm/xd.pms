using Microsoft.AspNetCore.Http;
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

namespace XD.Pms.Middlewares;

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

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);

			// 处理 401/403 等状态码（未被异常捕获的情况）
			if (!context.Response.HasStarted && IsApiRequest(context))
			{
				await HandleStatusCodeAsync(context);
			}
		}
		catch (Exception ex)
		{
			if (IsApiRequest(context) && !context.Response.HasStarted)
			{
				await HandleExceptionAsync(context, ex);
			}
			else
			{
				throw;
			}
		}
	}

	private static async Task HandleStatusCodeAsync(HttpContext context)
	{
		var statusCode = context.Response.StatusCode;

		// 只处理错误状态码
		if (statusCode >= 400 && context.Response.ContentLength == null)
		{
			var (code, message) = statusCode switch
			{
				401 => (ApiResponseCode.Unauthorized, "未经授权的访问，请先登录"),
				403 => (ApiResponseCode.Forbidden, "没有权限访问此资源"),
				404 => (ApiResponseCode.NotFound, "请求的资源不存在"),
				405 => (ApiResponseCode.BadRequest, "不支持的请求方法"),
				429 => (ApiResponseCode.TooManyRequests, "请求过于频繁，请稍后再试"),
				_ => (ApiResponseCode.InternalError, "请求失败")
			};

			await WriteResponseAsync(context, code, message);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		_logger.LogError(exception, "API 请求异常: {Path}", context.Request.Path);

		var (code, message) = exception switch
		{
			// 自定义认证异常
			AuthenticationException authEx
				=> (authEx.ErrorCode, authEx.Message),

			// ABP 业务异常
			UserFriendlyException userEx
				=> (ApiResponseCode.BadRequest, userEx.Message),

			// ABP 授权异常
			AbpAuthorizationException
				=> (ApiResponseCode.Forbidden, "没有权限执行此操作"),

			// ABP 验证异常
			AbpValidationException validationEx
				=> (ApiResponseCode.ValidationError, FormatValidationErrors(validationEx)),

			// 实体不存在
			EntityNotFoundException entityEx
				=> (ApiResponseCode.NotFound, $"请求的{GetEntityDisplayName(entityEx)}不存在"),

			// ABP 业务异常（通用）
			BusinessException bizEx
				=> (GetBusinessExceptionCode(bizEx), bizEx.Message),

			// 操作取消
			OperationCanceledException
				=> (ApiResponseCode.BadRequest, "操作已取消"),

			// 参数异常
			ArgumentException argEx
				=> (ApiResponseCode.ValidationError, argEx.Message),

			// 未处理的异常
			_ => (ApiResponseCode.InternalError, exception.Message ?? "服务器内部错误，请稍后重试")
		};

		await WriteResponseAsync(context, code, message);
	}

	private static async Task WriteResponseAsync(HttpContext context, string code, string message)
	{
		// 始终返回 200 状态码，通过 code 区分结果
		context.Response.StatusCode = 200;
		context.Response.ContentType = "application/json; charset=utf-8";

		var response = new ApiResponse<object>
		{
			Code = code,
			Data = null,
			Message = message
		};

		await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
	}

	private static bool IsApiRequest(HttpContext context)
	{
		var path = context.Request.Path.Value?.ToLower() ?? "";

		// API 路径
		if (path.StartsWith("/api/")) return true;

		// AJAX 请求
		if (context.Request.Headers.XRequestedWith == "XMLHttpRequest") return true;

		// Accept 包含 application/json
		var accept = context.Request.Headers.Accept.ToString();
		if (accept.Contains("application/json") && !accept.Contains("text/html")) return true;

		return false;
	}

	private static string FormatValidationErrors(AbpValidationException ex)
	{
		var errors = ex.ValidationErrors
			.SelectMany(e => e.MemberNames.Select(m => $"{m}: {e.ErrorMessage}"))
			.ToList();

		return errors.Count > 0 ? string.Join("; ", errors) : "参数验证失败";
	}

	private static string GetEntityDisplayName(EntityNotFoundException ex)
	{
		// 可以根据实体类型返回友好名称
		var entityType = ex.EntityType?.Name ?? "资源";
		return entityType switch
		{
			"IdentityUser" => "用户",
			"IdentityRole" => "角色",
			"Book" => "图书",
			_ => "数据"
		};
	}

	private static string GetBusinessExceptionCode(BusinessException ex)
	{
		// 根据异常代码映射
		return ex.Code switch
		{
			"Volo.Abp.Identity:DuplicateUserName" => ApiResponseCode.UsernameExists,
			"Volo.Abp.Identity:DuplicateEmail" => ApiResponseCode.EmailExists,
			_ => ApiResponseCode.BadRequest
		};
	}
}