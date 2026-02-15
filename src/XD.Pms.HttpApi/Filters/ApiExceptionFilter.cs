using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.ExceptionHandling;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.ExceptionHandling;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http;
using Volo.Abp.Validation;
using XD.Pms.ApiResponse;
using XD.Pms.Localization;

namespace XD.Pms.Filters;

/// <summary>
/// 自定义异常过滤器（替换 ABP 默认的 AbpExceptionFilter）
/// 处理 Controller 内部的所有异常
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(AbpExceptionFilter))]
public class ApiExceptionFilter : AbpExceptionFilter
{
	protected override async Task HandleAndWrapException(ExceptionContext context)
	{
		if (!IsApiRequest(context.HttpContext))
		{
			await base.HandleAndWrapException(context);
			return;
		}

		await HandleApiExceptionAsync(context);
	}

	/// <summary>
	/// 处理 API 异常
	/// </summary>
	private static async Task HandleApiExceptionAsync(ExceptionContext context)
	{
		var exception = context.Exception;
		var services = context.HttpContext.RequestServices;

		var logger = services.GetRequiredService<ILogger<ApiExceptionFilter>>();
		var localizer = services.GetRequiredService<IStringLocalizer<PmsResource>>();
		var exceptionConverter = services.GetRequiredService<IExceptionToErrorInfoConverter>();
		var exceptionOptions = services.GetRequiredService<IOptions<AbpExceptionHandlingOptions>>().Value;

		// 使用 ABP 的异常转换器获取本地化消息
		var errorInfo = exceptionConverter.Convert(exception, options =>
		{
			options.SendExceptionsDetailsToClients = exceptionOptions.SendExceptionsDetailsToClients;
			options.SendStackTraceToClients = exceptionOptions.SendStackTraceToClients;
		});

		// 记录异常
		LogException(context, exception, errorInfo, logger);

		// 通知异常
		await context.GetRequiredService<IExceptionNotifier>()
			.NotifyAsync(new ExceptionNotificationContext(exception));

		// 构建自定义响应
		var (code, message) = ConvertException(exception, errorInfo, localizer);
		var response = ApiResponse<object>.Fail(code, message);

		context.HttpContext.Response.StatusCode = StatusCodes.Status200OK;
		context.HttpContext.Response.Headers.Remove(AbpHttpConsts.AbpErrorFormat);
		context.Result = new ObjectResult(response);
		context.ExceptionHandled = true;
	}

	private static (string Code, string Message) ConvertException(
		Exception exception,
		RemoteServiceErrorInfo errorInfo,
		IStringLocalizer<PmsResource> localizer)
	{
		return exception switch
		{
			PmsBusinessException pmsEx => (pmsEx.ErrorCode, pmsEx.Message),

			AbpAuthorizationException => (ApiResponseCode.Forbidden, localizer["Auth:Forbidden"].Value),

			BusinessException bizEx => (ApiResponseCode.BadRequest, errorInfo.Message ?? bizEx.Message),

			AbpValidationException validationEx => (ApiResponseCode.ValidationError, FormatValidationErrors(validationEx, errorInfo)),

			EntityNotFoundException => (ApiResponseCode.NotFound, localizer["Auth:NotFound"].Value),

			ArgumentException argEx => (ApiResponseCode.BadRequest, argEx.Message),

			OperationCanceledException ocEx => (ApiResponseCode.BadRequest, ocEx.Message),

			AbpException abpEx => (ApiResponseCode.BadRequest, abpEx.Message),

			_ => (ApiResponseCode.InternalError, localizer["Auth:ServerError"].Value)
		};
	}

	private static string FormatValidationErrors(AbpValidationException exception, RemoteServiceErrorInfo errorInfo)
	{
		if (errorInfo.ValidationErrors != null && errorInfo.ValidationErrors.Length > 0)
		{
			var errors = errorInfo.ValidationErrors.SelectMany(e => e.Members.Select(m => $"{m}: {e.Message}"));
			return string.Join("; ", errors);
		}

		if (exception.ValidationErrors != null && exception.ValidationErrors.Count > 0)
		{
			var errors = exception.ValidationErrors.SelectMany(e => e.MemberNames.Select(m => $"{m}: {e.ErrorMessage}"));
			return string.Join("; ", errors);
		}

		return exception.Message;
	}

	private static void LogException(
		ExceptionContext context, 
		Exception exception,
		RemoteServiceErrorInfo errorInfo,
		ILogger<ApiExceptionFilter> logger)
	{
		var logBuilder = new StringBuilder();
		logBuilder.AppendLine("---------- API Exception ----------");
		logBuilder.AppendLine($"Request Path: {context.HttpContext.Request.Path}");
		if (exception is PmsBusinessException pmsEx && !string.IsNullOrEmpty(pmsEx.Details))
		{
			logBuilder.Append($"Details: {pmsEx.Details}");
		}
		if (errorInfo.ValidationErrors != null && errorInfo.ValidationErrors.Length > 0)
		{
			var errors = errorInfo.ValidationErrors.SelectMany(e => e.Members.Select(m => $"{m}: {e.Message}"));
			logBuilder.Append($"Validation Errors: {errors}"); 
		}
		logger.LogWarning(exception, "{ExceptionInfo}", logBuilder.ToString());
	}

	private static bool IsApiRequest(HttpContext context)
	{
		var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";
		return path.StartsWith("/papi/") || path.StartsWith("/api/");
	}
}
