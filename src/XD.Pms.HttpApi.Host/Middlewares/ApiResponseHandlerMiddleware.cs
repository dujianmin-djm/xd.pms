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
using Volo.Abp.Json;
using Volo.Abp.Validation;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;
using XD.Pms.Localization;

namespace XD.Pms.Middlewares;

/// <summary>
/// API 异常处理中间件
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

			// 处理 401/403 等状态码
			if (!context.Response.HasStarted && IsApiRequest(context))
			{
				if (context.Response.StatusCode >= 400 
					&& (context.Response.ContentLength == null || context.Response.ContentLength == 0))
				{
					await HandleStatusCodeAsync(context);
				}
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

		if (statusCode == 401)
		{
			// 获取详细的认证错误信息
			var analysisResult = await AnalyzeAuthenticationErrorAsync(context);
			code = analysisResult.Code;
			message = analysisResult.Message;

			_logger.LogWarning(
				"Authentication failed for {Path}. Status: {Status}, Code: {Code}, Details: {Details}",
				context.Request.Path, analysisResult.Status, code, analysisResult.Details);
		}
		else if (statusCode == 403)
		{
			code = ApiResponseCode.Forbidden;
			message = _localizer["Auth:Forbidden"].Value;
		}
		else
		{
			(code, message) = GetStatusCodeResponse(statusCode);
		}

		await WriteResponseAsync(context, code, message);
	}

	private async Task<TokenAnalysisResult> AnalyzeAuthenticationErrorAsync(HttpContext context)
	{
		// 获取请求中的 Token
		var authHeader = context.Request.Headers.Authorization.ToString();
		if (!authHeader.IsNullOrEmpty() && authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
		{
			var token = authHeader.Substring("Bearer ".Length).Trim();
			var wwwAuthenticate = context.Response.Headers.WWWAuthenticate.ToString();

			// 使用 TokenAnalysisService 进行分析
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

	private (string Code, string Message) GetStatusCodeResponse(int statusCode)
	{
		return statusCode switch
		{
			400 => (ApiResponseCode.BadRequest, _localizer["Auth:BadRequest"].Value),
			404 => (ApiResponseCode.NotFound, _localizer["Auth:NotFound"].Value),
			405 => (ApiResponseCode.MethodNotAllowed, _localizer["Auth:MethodNotAllowed"].Value),
			429 => (ApiResponseCode.TooManyRequests, _localizer["Auth:TooManyRequests"].Value),
			_ => (ApiResponseCode.InternalError, _localizer["Auth:ServerError"].Value)
		};
	}

	private async Task HandleExceptionAsync(HttpContext context, Exception exception)
	{
		_logger.LogError(exception, "API 请求异常: {Path}", context.Request.Path);

		var (code, message) = exception switch
		{
			// 业务异常
			BusinessException bizEx => (bizEx.Code ?? ApiResponseCode.BadRequest, bizEx.Message),

			// 授权异常
			AbpAuthorizationException => (ApiResponseCode.Forbidden, _localizer["Auth:Forbidden"].Value),

			// 验证异常
			AbpValidationException validationEx => (ApiResponseCode.ValidationError, FormatValidationErrors(validationEx)),

			// 实体不存在
			EntityNotFoundException => (ApiResponseCode.NotFound, _localizer["Auth:NotFound"].Value),

			// 操作取消
			OperationCanceledException ocEx => (ApiResponseCode.BadRequest, ocEx.Message),

			// 非法操作
			InvalidOperationException ioEx => (ApiResponseCode.BadRequest, ioEx.Message),

			// 参数异常
			ArgumentException argEx => (ApiResponseCode.BadRequest, argEx.Message),

			// 未处理的异常
			_ => (ApiResponseCode.InternalError, _localizer["Auth:ServerError"].Value)
		};

		await WriteResponseAsync(context, code, message);
	}

	private static async Task WriteResponseAsync(HttpContext context, string code, string message)
	{
		context.Response.StatusCode = 200;
		context.Response.ContentType = "application/json; charset=utf-8";
		var jsonSerializer = context.RequestServices.GetService<IJsonSerializer>()!;
		var response = jsonSerializer.Serialize(ApiResponse<object>.Fail(code, message));
		await context.Response.WriteAsync(response);
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