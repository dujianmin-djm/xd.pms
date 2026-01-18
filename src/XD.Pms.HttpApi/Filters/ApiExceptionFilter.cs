using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.ExceptionHandling;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.ExceptionHandling;
using Volo.Abp.Http;
using Volo.Abp.Validation;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;
using XD.Pms.Localization;

namespace XD.Pms.Filters;

/// <summary>
/// 自定义异常过滤器（替换 ABP 默认的 AbpExceptionFilter）
/// </summary>
[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(AbpExceptionFilter))]
public class ApiExceptionFilter : AbpExceptionFilter
{
	protected override async Task HandleAndWrapException(ExceptionContext context)
	{
		// 判断是否是 API 请求
		if (!IsApiRequest(context.HttpContext))
		{
			// 非 API 请求，使用 ABP 默认处理
			await base.HandleAndWrapException(context);
			return;
		}

		// API 请求，使用自定义格式
		await HandleApiExceptionAsync(context);
	}

	/// <summary>
	/// 处理 API 异常
	/// </summary>
	private async Task HandleApiExceptionAsync(ExceptionContext context)
	{
		// 记录异常
		LogException(context, out _);

		// 通知异常
		await context.GetRequiredService<IExceptionNotifier>()
			.NotifyAsync(new ExceptionNotificationContext(context.Exception));

		// 获取本地化器
		var localizer = context.HttpContext.RequestServices
			.GetRequiredService<IStringLocalizer<PmsResource>>();

		// 构建自定义响应
		var (code, message) = ConvertException(context.Exception, localizer);

		var response = new ApiResponse<object>(code, false, null, message);

		// 设置响应
		context.HttpContext.Response.StatusCode = 200;  // 始终返回 200
		context.HttpContext.Response.Headers.Remove(AbpHttpConsts.AbpErrorFormat);  // 移除 ABP 错误头

		context.Result = new ObjectResult(response);
		context.ExceptionHandled = true;
	}

	/// <summary>
	/// 转换异常为错误码和消息
	/// </summary>
	private static (string Code, string Message) ConvertException(
		Exception exception,
		IStringLocalizer<PmsResource> localizer)
	{
		return exception switch
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

			// 用户友好异常
			UserFriendlyException userEx
				=> (ApiResponseCode.BadRequest, userEx.Message),

			// 业务异常
			BusinessException businessEx
				=> (GetBusinessExceptionCode(businessEx), businessEx.Message),

			// 参数异常
			ArgumentException argEx
				=> (ApiResponseCode.ValidationError, argEx.Message),

			// 操作取消
			OperationCanceledException
				=> (ApiResponseCode.BadRequest, "Operation Canceled"),

			// 未知异常
			_ => (ApiResponseCode.InternalError, localizer["Auth:ServerError"].Value)
		};
	}

	/// <summary>
	/// 获取业务异常的错误码
	/// </summary>
	private static string GetBusinessExceptionCode(BusinessException exception)
	{
		// 根据 ABP 异常代码映射
		return exception.Code switch
		{
			"Volo.Abp.Identity:DuplicateUserName" => ApiResponseCode.UsernameExists,
			"Volo.Abp.Identity:DuplicateEmail" => ApiResponseCode.EmailExists,
			"Volo.Abp.Identity:InvalidPassword" => ApiResponseCode.ValidationError,
			"Volo.Abp.Identity:PasswordMismatch" => ApiResponseCode.InvalidCredentials,
			"Volo.Abp.Identity:UserLockedOut" => ApiResponseCode.AccountLocked,
			_ => ApiResponseCode.BadRequest
		};
	}

	/// <summary>
	/// 格式化验证错误
	/// </summary>
	private static string FormatValidationErrors(AbpValidationException exception)
	{
		if (exception.ValidationErrors == null || exception.ValidationErrors.Count == 0)
		{
			return exception.Message;
		}

		var errors = exception.ValidationErrors
			.SelectMany(e => e.MemberNames.Select(m => $"{m}: {e.ErrorMessage}"))
			.ToList();

		return string.Join("; ", errors);
	}

	/// <summary>
	/// 判断是否是 API 请求
	/// </summary>
	private static bool IsApiRequest(HttpContext context)
	{
		var path = context.Request.Path.Value?.ToLowerInvariant() ?? "";

		// 路径判断
		if (path.StartsWith("/api/"))
		{
			return true;
		}

		// AJAX 请求
		if (context.Request.Headers.XRequestedWith == "XMLHttpRequest")
		{
			return true;
		}

		// Accept 头判断
		var accept = context.Request.Headers.Accept.ToString();
		if (accept.Contains("application/json") && !accept.Contains("text/html"))
		{
			return true;
		}

		return false;
	}
}
