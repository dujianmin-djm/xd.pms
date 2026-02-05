using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenIddict.Abstractions;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Volo.Abp.Security.Claims;
using XD.Pms.ApiKeys;

namespace XD.Pms.Authentication.ApiKey;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
	private readonly IApiKeyValidator _apiKeyValidator;

	public ApiKeyAuthenticationHandler(
		IOptionsMonitor<ApiKeyAuthenticationOptions> options,
		ILoggerFactory logger,
		UrlEncoder encoder,
		IApiKeyValidator apiKeyValidator)
		: base(options, logger, encoder)
	{
		_apiKeyValidator = apiKeyValidator;
	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		// 检查是否启用
		if (!Options.Enabled)
		{
			return AuthenticateResult.NoResult();
		}

		// 从 Header 获取 API Key
		if (!Request.Headers.TryGetValue(Options.HeaderName, out var apiKeyHeader))
		{
			return AuthenticateResult.NoResult();
		}

		var apiKey = apiKeyHeader.ToString();
		if (string.IsNullOrWhiteSpace(apiKey))
		{
			return AuthenticateResult.NoResult();
		}

		// 获取请求 IP
		var ipAddress = GetClientIpAddress();

		// 验证
		var result = await _apiKeyValidator.ValidateAsync(apiKey, ipAddress);

		if (!result.IsValid)
		{
			Logger.LogWarning("API Key 验证失败: {Message}", result.ErrorMessage);
			return AuthenticateResult.Fail(result.ErrorMessage ?? "Invalid API Key");
		}

		// 创建 Claims
		var claims = new List<Claim>
		{
			new(OpenIddictConstants.Claims.Subject, result.ClientId!),
			new(OpenIddictConstants.Claims.Name, result.ClientName!),
			new(ClaimTypes.NameIdentifier, result.ClientId!),
			new(ClaimTypes.Name, result.ClientName!),
			new("api_key_id", result.ApiKeyId.ToString()!),
			new("client_id", result.ClientId!),
			new("auth_type", "api_key")
		};

		// 添加用户（如果有关联）
		if (result.UserId.HasValue)
		{
			claims.Add(new Claim(AbpClaimTypes.UserId, result.UserId.Value.ToString()));
		}

		// 添加角色
		if (result.Roles != null)
		{
			foreach (var role in result.Roles)
			{
				claims.Add(new Claim(ClaimTypes.Role, role));
				claims.Add(new Claim(OpenIddictConstants.Claims.Role, role));
			}
		}

		// 添加权限
		if (result.Permissions != null)
		{
			foreach (var permission in result.Permissions)
			{
				claims.Add(new Claim(AbpClaimTypes.Role, permission));
			}
		}

		var identity = new ClaimsIdentity(claims, Scheme.Name);
		var principal = new ClaimsPrincipal(identity);
		var ticket = new AuthenticationTicket(principal, Scheme.Name);

		if (Logger.IsEnabled(LogLevel.Debug))
		{
			Logger.LogDebug("API Key 验证成功: {ClientId}", result.ClientId);
		}

		return AuthenticateResult.Success(ticket);
	}

	protected override Task HandleChallengeAsync(AuthenticationProperties properties)
	{
		Response.StatusCode = 401;
		Response.Headers.WWWAuthenticate = $"ApiKey realm=\"{Options.Realm}\"";
		return Task.CompletedTask;
	}

	protected override Task HandleForbiddenAsync(AuthenticationProperties properties)
	{
		Response.StatusCode = 403;
		return Task.CompletedTask;
	}

	private string? GetClientIpAddress()
	{
		// 检查代理头
		var forwardedFor = Request.Headers["X-Forwarded-For"].FirstOrDefault();
		if (!string.IsNullOrEmpty(forwardedFor))
		{
			return forwardedFor.Split(',').First().Trim();
		}

		var realIp = Request.Headers["X-Real-IP"].FirstOrDefault();
		if (!string.IsNullOrEmpty(realIp))
		{
			return realIp;
		}

		return Context.Connection.RemoteIpAddress?.ToString();
	}
}