using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace XD.Pms.Web.Middlewares;

/// <summary>
/// 语言设置中间件
/// </summary>
public class ApiLanguageSettingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ApiLanguageSettingMiddleware> _logger;

	public ApiLanguageSettingMiddleware(RequestDelegate next, ILogger<ApiLanguageSettingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		// 设置语言
		await SetCultureAsync(context);

		await _next(context);
	}

	private async Task SetCultureAsync(HttpContext context)
	{
		// 优先级 1：请求头 X-Language
		string? language = context.Request.Headers["X-Language"].FirstOrDefault();
		if (TrySetCulture(language))
		{
			_logger.LogDebug("Language set from X-Language header: {Language}", language);
			return;
		}

		// 优先级 2：请求体中的 language 字段（仅限 POST/PUT 请求）
		if (context.Request.Method is "POST" or "PUT" &&
			context.Request.ContentType?.Contains("application/json") == true)
		{
			language = await TryGetLanguageFromBodyAsync(context);
			if (TrySetCulture(language))
			{
				_logger.LogDebug("Language set from request body: {Language}", language);
				return;
			}
		}

		// 优先级 3：查询参数 lang
		language = context.Request.Query["lang"].FirstOrDefault();
		if (TrySetCulture(language))
		{
			_logger.LogDebug("Language set from query parameter: {Language}", language);
			return;
		}

		// 优先级 4：Accept-Language 头
		var acceptLanguage = context.Request.Headers.AcceptLanguage.FirstOrDefault();
		if (!string.IsNullOrEmpty(acceptLanguage))
		{
			language = acceptLanguage.Split(',').FirstOrDefault()?.Split(';').FirstOrDefault()?.Trim();
			if (TrySetCulture(language))
			{
				_logger.LogDebug("Language set from Accept-Language header: {Language}", language);
				return;
			}
		}

		// 默认语言
		_logger.LogDebug("Using default language: zh-Hans");
	}

	private async Task<string?> TryGetLanguageFromBodyAsync(HttpContext context)
	{
		try
		{
			// 启用请求体缓冲
			context.Request.EnableBuffering();

			using var reader = new StreamReader(
				context.Request.Body,
				Encoding.UTF8,
				detectEncodingFromByteOrderMarks: false,
				leaveOpen: true);

			var body = await reader.ReadToEndAsync();

			// 重置位置以便后续读取
			context.Request.Body.Position = 0;

			if (string.IsNullOrEmpty(body))
			{
				return null;
			}

			// 解析 JSON 获取 language 字段
			using var doc = JsonDocument.Parse(body);
			if (doc.RootElement.TryGetProperty("language", out var languageElement))
			{
				return languageElement.GetString();
			}
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "Failed to parse language from request body");
		}

		return null;
	}

	private static bool TrySetCulture(string? language)
	{
		if (string.IsNullOrWhiteSpace(language))
		{
			return false;
		}

		try
		{
			var culture = CultureInfo.GetCultureInfo(language);
			CultureInfo.CurrentCulture = culture;
			CultureInfo.CurrentUICulture = culture;
			return true;
		}
		catch (CultureNotFoundException)
		{
			return false;
		}
	}
}