using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
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
			// 从请求头获取语言设置
			SetCultureFromRequest(context);

			await _next(context);

			// 处理 401/403 等状态码（未被异常捕获的情况）
			if (!context.Response.HasStarted && IsApiRequest(context))
			{
				await HandleStatusCodeAsync(context, localizer);
			}
		}
		catch (Exception ex)
		{
			if (IsApiRequest(context) && !context.Response.HasStarted)
			{
				await HandleExceptionAsync(context, ex, localizer);
			}
			else
			{
				throw;
			}
		}
	}

	/// <summary>
	/// 从请求中设置语言环境
	/// </summary>
	private static void SetCultureFromRequest(HttpContext context)
	{
		// 优先级：X-Language 头 > Accept-Language 头

		// 1. 自定义头 X-Language
		var xLanguage = context.Request.Headers["X-Language"].FirstOrDefault();
		if (!string.IsNullOrEmpty(xLanguage))
		{
			TrySetCulture(xLanguage);
			return;
		}

		// 2. 标准头 Accept-Language
		var acceptLanguage = context.Request.Headers.AcceptLanguage.FirstOrDefault();
		if (!string.IsNullOrEmpty(acceptLanguage))
		{
			// Accept-Language 可能包含多个语言和权重，取第一个
			var language = acceptLanguage.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault()?.Trim();
			if (!string.IsNullOrEmpty(language))
			{
				TrySetCulture(language);
			}
		}
	}

	private static void TrySetCulture(string language)
	{
		try
		{
			var culture = CultureInfo.GetCultureInfo(language);
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;
		}
		catch (CultureNotFoundException)
		{
			// 忽略无效语言
		}
	}

	private static async Task HandleStatusCodeAsync(HttpContext context, IStringLocalizer<PmsResource> localizer)
	{
		var statusCode = context.Response.StatusCode;

		// 只处理错误状态码
		if (statusCode >= 400 && context.Response.ContentLength == null)
		{
			var (code, messageKey) = statusCode switch
			{
				401 => (ApiResponseCode.Unauthorized, "Auth:Unauthorized"),
				403 => (ApiResponseCode.Forbidden, "Auth:Forbidden"),
				404 => (ApiResponseCode.NotFound, "Auth:NotFound"),
				405 => (ApiResponseCode.BadRequest, "Auth:BadRequest"),
				429 => (ApiResponseCode.TooManyRequests, "Auth:TooManyRequests"),
				_ => (ApiResponseCode.InternalError, "Auth:OperationFailed")
			};

			await WriteResponseAsync(context, code, localizer[messageKey].Value);
		}
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception, IStringLocalizer<PmsResource> localizer)
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
				=> (ApiResponseCode.Forbidden, localizer["Auth:Forbidden"].Value),

			// ABP 验证异常
			AbpValidationException validationEx
				=> (ApiResponseCode.ValidationError, FormatValidationErrors(validationEx)),

			// 实体不存在
			EntityNotFoundException
				=> (ApiResponseCode.NotFound, localizer["Auth:NotFound"].Value),

			// ABP 业务异常（通用）
			BusinessException bizEx
				=> (GetBusinessExceptionCode(bizEx), bizEx.Message),

			// 操作取消
			OperationCanceledException
				=> (ApiResponseCode.BadRequest, "Operation Canceled"),

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
		// 始终返回 200 状态码，通过 code 区分结果
		context.Response.StatusCode = 200;
		context.Response.ContentType = "application/json; charset=utf-8";

		var response = ApiResponse<object>.Fail(code, message);

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

		return string.Join("; ", errors);
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