using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Json;
using Volo.Abp.Validation;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;
using XD.Pms.Localization;

namespace XD.Pms.Middlewares;

/// <summary>
/// API 异常处理中间件
/// 1. 处理 401/403 状态码
/// 2. 捕获 MVC 管道外部的异常
/// 3. 处理所有其他未被处理的异常
/// </summary>
public class ApiResponseHandlerMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IStringLocalizer<PmsResource> _localizer;
	private readonly ILogger<ApiResponseHandlerMiddleware> _logger;

	public ApiResponseHandlerMiddleware(
		RequestDelegate next,
		IStringLocalizer<PmsResource> localizer,
		ILogger<ApiResponseHandlerMiddleware> logger)
	{
		_next = next;
		_logger = logger;
		_localizer = localizer;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);

			if (IsApiRequest(context) && 
				!context.Response.HasStarted &&
				context.Response.StatusCode >= 400 &&
				(context.Response.ContentLength == null || context.Response.ContentLength == 0))
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

	private async Task HandleStatusCodeAsync(HttpContext context)
	{
		var statusCode = context.Response.StatusCode;
		
		string code;
		string message;

		switch (statusCode)
		{
			case StatusCodes.Status401Unauthorized:
				// 分析401错误的具体原因
				var analysisResult = await AnalyzeAuthenticationErrorAsync(context);
				code = analysisResult.Code;
				message = analysisResult.Message;
				if (_logger.IsEnabled(LogLevel.Information))
				{
					_logger.LogInformation(
						"认证失败。Path: {Path}, Status: {Status}, Code: {Code}, Details: {Details}",
						context.Request.Path, analysisResult.Status, code, analysisResult.Details);
				}
				break;

			case StatusCodes.Status403Forbidden:
				code = ApiResponseCode.Forbidden;
				message = _localizer["Auth:Forbidden"].Value;
				break;

			case StatusCodes.Status404NotFound:
				code = ApiResponseCode.NotFound;
				message = _localizer["Auth:NotFound"].Value;
				break;

			case StatusCodes.Status405MethodNotAllowed:
				code = ApiResponseCode.MethodNotAllowed;
				message = _localizer["Auth:MethodNotAllowed"].Value;
				break;

			case StatusCodes.Status429TooManyRequests:
				code = ApiResponseCode.TooManyRequests;
				message = _localizer["Auth:TooManyRequests"].Value;
				break;

			default:
				code = statusCode.ToString();
				message = _localizer["Auth:UnexpectedError"].Value;
				break;
		}

		await WriteResponseAsync(context, code, message);
	}

	private async Task<TokenAnalysisResult> AnalyzeAuthenticationErrorAsync(HttpContext context)
	{
		var authHeader = context.Request.Headers.Authorization.ToString();
		if (!authHeader.IsNullOrEmpty() && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
		{
			var token = authHeader.Substring("Bearer ".Length).Trim();
			var wwwAuthenticate = context.Response.Headers.WWWAuthenticate.ToString();
			var analysisService = context.RequestServices.GetService<ITokenAnalysisService>();
			if (analysisService != null)
			{
				return await analysisService.AnalyzeTokenErrorAsync(token, wwwAuthenticate);
			}
		}
		return new TokenAnalysisResult
		{
			Code = ApiResponseCode.Unauthorized,
			Message = _localizer["Auth:AccessTokenMissing"].Value,
			Status = TokenStatus.Missing
		};
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		_logger.LogError(exception, "中间件捕获未处理异常。API Path: {Path}", context.Request.Path);

		var (code, message) = exception switch
		{
			PmsBusinessException pmsEx => (pmsEx.ErrorCode, pmsEx.Message),
			AbpAuthorizationException => (ApiResponseCode.Forbidden, _localizer["Auth:Forbidden"].Value),
			BusinessException bizEx => (ApiResponseCode.BadRequest, bizEx.Message),
			AbpValidationException validationEx => (ApiResponseCode.ValidationError, FormatValidationErrors(validationEx)),
			EntityNotFoundException => (ApiResponseCode.NotFound, _localizer["Auth:NotFound"].Value),
			OperationCanceledException ocEx => (ApiResponseCode.BadRequest, ocEx.Message),
			_ => (ApiResponseCode.InternalError, _localizer["Auth:ServerError"].Value)
		};

		var notifier = context.RequestServices.GetService<IExceptionNotifier>();
		if (notifier != null)
		{
			await notifier.NotifyAsync(new ExceptionNotificationContext(exception));
		}

		await WriteResponseAsync(context, code, message);
	}

	private static async Task WriteResponseAsync(HttpContext context, string code, string message)
	{
		context.Response.StatusCode = StatusCodes.Status200OK;
		context.Response.ContentType = "application/json; charset=utf-8";
		var jsonSerializer = context.RequestServices.GetRequiredService<IJsonSerializer>();
		var response = jsonSerializer.Serialize(ApiResponse<object>.Fail(code, message));
		await context.Response.WriteAsync(response);
	}

	private static string FormatValidationErrors(AbpValidationException ex)
	{
		var errors = ex.ValidationErrors.SelectMany(e => e.MemberNames.Select(m => $"{m}: {e.ErrorMessage}"));
		return string.Join("; ", errors);
	}

	private static bool IsApiRequest(HttpContext context)
	{
		var path = context.Request.Path.Value?.ToLower() ?? "";
		return path.StartsWith("/papi/") || path.StartsWith("/api/");
	}
}