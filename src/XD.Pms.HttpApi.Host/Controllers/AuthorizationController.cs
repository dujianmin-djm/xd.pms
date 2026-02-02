using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OpenIddict.Abstractions;
using OpenIddict.Server.AspNetCore;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Identity;
using Volo.Abp.Security.Claims;
using IdentityUser = Volo.Abp.Identity.IdentityUser;

namespace XD.Pms.Controllers;

[Route("connect")]
[ApiExplorerSettings(IgnoreApi = true)]
public class AuthorizationController : AbpController
{
	private readonly SignInManager<IdentityUser> _signInManager;
	private readonly IdentityUserManager _userManager;
	private readonly IOpenIddictScopeManager _scopeManager;

	public AuthorizationController(
		SignInManager<IdentityUser> signInManager,
		IdentityUserManager userManager,
		IOpenIddictScopeManager scopeManager)
	{
		_signInManager = signInManager;
		_userManager = userManager;
		_scopeManager = scopeManager;
	}

	/// <summary>
	/// 授权端点 - GET（显示授权页面或自动授权）
	/// </summary>
	[HttpGet("authorize2")]
	[HttpPost("authorize2")]
	public async Task<IActionResult> Authorize()
	{
		var request = HttpContext.GetOpenIddictServerRequest()
			?? throw new InvalidOperationException("无法获取 OpenIddict 请求");

		// 检查用户是否已通过 Cookie 认证
		var result = await HttpContext.AuthenticateAsync(IdentityConstants.ApplicationScheme);

		if (!result.Succeeded || request.HasPromptValue(OpenIddictConstants.PromptValues.Login))
		{
			var parameters = Request.HasFormContentType
				? Request.Form.Where(p => p.Key != "__RequestVerificationToken")
				: Request.Query;

			var redirectUri = Request.PathBase + Request.Path + QueryString.Create(parameters);

			// 重定向到登录页面
			return Challenge(
				authenticationSchemes: IdentityConstants.ApplicationScheme,
				properties: new AuthenticationProperties
				{
					RedirectUri = redirectUri
				});
		}

		// 获取用户
		var userId = result.Principal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
		if (string.IsNullOrEmpty(userId))
		{
			throw new InvalidOperationException("无法获取用户 ID");
		}

		var user = await _userManager.FindByIdAsync(userId) 
			?? throw new InvalidOperationException("用户不存在");

		var principal = await CreateClaimsPrincipalAsync(user, request);

		return SignIn(principal, OpenIddictServerAspNetCoreDefaults.AuthenticationScheme);
	}

	/// <summary>
	/// 显示登录页面
	/// </summary>
	[HttpGet("login")]
	[AllowAnonymous]
	public IActionResult Login([FromQuery] string? returnUrl = null)
	{
		ViewData["ReturnUrl"] = returnUrl;
		return Content(GetLoginPage(returnUrl, null), "text/html");
	}

	/// <summary>
	/// 处理登录请求
	/// </summary>
	[HttpPost("login")]
	[AllowAnonymous]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> LoginPost([FromForm] string username, [FromForm] string password, [FromForm] string? returnUrl = null)
	{
		if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
		{
			return Content(GetLoginPage(returnUrl, "请输入用户名和密码"), "text/html");
		}
		var user = await _userManager.FindByNameAsync(username)
			?? await _userManager.FindByEmailAsync(username);

		if (user == null)
		{
			return Content(GetLoginPage(returnUrl, "用户名或密码错误"), "text/html");
		}

		// 检查用户状态
		if (!user.IsActive)
		{
			return Content(GetLoginPage(returnUrl, "账户已被禁用"), "text/html");
		}

		// 验证密码
		var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: true);

		if (result.IsLockedOut)
		{
			return Content(GetLoginPage(returnUrl, "账户已被锁定，请稍后重试"), "text/html");
		}

		if (!result.Succeeded)
		{
			return Content(GetLoginPage(returnUrl, "用户名或密码错误"), "text/html");
		}

		// 登录成功，创建身份认证 Cookie
		await _signInManager.SignInAsync(user, isPersistent: false);

		// 重定向
		if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
		{
			return Redirect(returnUrl);
		}

		return Redirect("/");
	}

	/// <summary>
	/// 登出
	/// </summary>
	[HttpGet("logout")]
	[HttpPost("logout")]
	[IgnoreAntiforgeryToken]
	public async Task<IActionResult> Logout()
	{
		await _signInManager.SignOutAsync();

		return SignOut(
			authenticationSchemes: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
			properties: new AuthenticationProperties { RedirectUri = "/" });
	}

	private async Task<ClaimsPrincipal> CreateClaimsPrincipalAsync(IdentityUser user, OpenIddictRequest request)
	{
		var identity = new ClaimsIdentity(
			authenticationType: OpenIddictServerAspNetCoreDefaults.AuthenticationScheme,
			nameType: OpenIddictConstants.Claims.Name,
			roleType: OpenIddictConstants.Claims.Role);

		// 基本 Claims
		identity.SetClaim(OpenIddictConstants.Claims.Subject, user.Id.ToString());
		identity.SetClaim(OpenIddictConstants.Claims.Name, user.UserName);
		identity.SetClaim(OpenIddictConstants.Claims.PreferredUsername, user.UserName);

		if (!string.IsNullOrEmpty(user.Email))
		{
			identity.SetClaim(OpenIddictConstants.Claims.Email, user.Email);
			identity.SetClaim(OpenIddictConstants.Claims.EmailVerified,
				user.EmailConfirmed.ToString().ToLowerInvariant());
		}

		if (!string.IsNullOrEmpty(user.PhoneNumber))
		{
			identity.SetClaim(OpenIddictConstants.Claims.PhoneNumber, user.PhoneNumber);
		}

		// 租户
		if (user.TenantId.HasValue)
		{
			identity.SetClaim(AbpClaimTypes.TenantId, user.TenantId.Value.ToString());
		}

		// 角色
		var roles = await _userManager.GetRolesAsync(user);
		identity.SetClaims(OpenIddictConstants.Claims.Role, roles.ToImmutableArray());

		var principal = new ClaimsPrincipal(identity);

		// 设置 Scopes
		var scopes = request.GetScopes();
		principal.SetScopes(scopes);

		// 设置 Resources
		principal.SetResources(await GetResourcesAsync(scopes));

		// 设置 Claims 目标
		principal.SetDestinations(GetDestinations);

		return principal;
	}

	private async Task<IEnumerable<string>> GetResourcesAsync(ImmutableArray<string> scopes)
	{
		var resources = new List<string> { "Pms" };

		await foreach (var resource in _scopeManager.ListResourcesAsync(scopes))
		{
			if (!resources.Contains(resource))
			{
				resources.Add(resource);
			}
		}

		return resources;
	}

	private static IEnumerable<string> GetDestinations(Claim claim)
	{
		return claim.Type switch
		{
			OpenIddictConstants.Claims.Name or
			OpenIddictConstants.Claims.PreferredUsername or
			OpenIddictConstants.Claims.Email or
			OpenIddictConstants.Claims.EmailVerified or
			OpenIddictConstants.Claims.PhoneNumber or
			OpenIddictConstants.Claims.Role
				=> [OpenIddictConstants.Destinations.AccessToken, OpenIddictConstants.Destinations.IdentityToken],
			_ => [OpenIddictConstants.Destinations.AccessToken]
		};
	}

	private static string GetLoginPage(string? returnUrl, string? error)
	{
		var errorHtml = string.IsNullOrEmpty(error) ? "" : $"<div class='error'>{error}</div>";

		var returnUrlValue = System.Web.HttpUtility.HtmlEncode(returnUrl ?? "");

		return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1'>
    <title>用户登录</title>
    <style>
        * {{ box-sizing: border-box; }}
        body {{ 
            font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Oxygen, Ubuntu, sans-serif;
            display: flex; 
            justify-content: center; 
            align-items: center; 
            min-height: 100vh; 
            margin: 0; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        }}
        .container {{ 
            background: white; 
            padding: 40px; 
            border-radius: 12px; 
            box-shadow: 0 10px 40px rgba(0,0,0,0.2); 
            width: 100%;
            max-width: 380px; 
        }}
        h2 {{ 
            text-align: center; 
            color: #333; 
            margin: 0 0 30px 0;
            font-weight: 600;
        }}
        .form-group {{
            margin-bottom: 20px;
        }}
        label {{
            display: block;
            margin-bottom: 6px;
            color: #555;
            font-size: 14px;
            font-weight: 500;
        }}
        input[type='text'],
        input[type='password'] {{ 
            width: 100%; 
            padding: 12px 16px; 
            border: 2px solid #e1e1e1; 
            border-radius: 8px; 
            font-size: 16px;
            transition: border-color 0.2s, box-shadow 0.2s;
        }}
        input[type='text']:focus,
        input[type='password']:focus {{
            outline: none;
            border-color: #667eea;
            box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
        }}
        button {{ 
            width: 100%; 
            padding: 14px; 
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white; 
            border: none; 
            border-radius: 8px; 
            cursor: pointer; 
            font-size: 16px;
            font-weight: 600;
            transition: transform 0.2s, box-shadow 0.2s;
            margin-top: 10px;
        }}
        button:hover {{ 
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.4);
        }}
        button:active {{
            transform: translateY(0);
        }}
        .error {{ 
            color: #dc3545; 
            background: #ffe6e6;
            padding: 12px;
            border-radius: 8px;
            text-align: center; 
            margin-bottom: 20px;
            font-size: 14px;
        }}
        .logo {{
            text-align: center;
            margin-bottom: 20px;
        }}
        .logo svg {{
            width: 60px;
            height: 60px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='logo'>
            <svg viewBox='0 0 24 24' fill='none' stroke='#667eea' stroke-width='2'>
                <path d='M12 2L2 7l10 5 10-5-10-5z'/>
                <path d='M2 17l10 5 10-5'/>
                <path d='M2 12l10 5 10-5'/>
            </svg>
        </div>
        <h2>用户登录</h2>
        {errorHtml}
        <form method='post' action='/connect/login'>
            <input type='hidden' name='returnUrl' value='{returnUrlValue}' />
            <div class='form-group'>
                <label for='username'>用户名</label>
                <input type='text' id='username' name='username' placeholder='请输入用户名' required autofocus />
            </div>
            <div class='form-group'>
                <label for='password'>密码</label>
                <input type='password' id='password' name='password' placeholder='请输入密码' required />
            </div>
            <button type='submit'>登 录</button>
        </form>
    </div>
</body>
</html>";
	}
}