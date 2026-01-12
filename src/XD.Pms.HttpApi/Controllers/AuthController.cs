using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using XD.Pms.Authentication;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Controllers;

[Area("app")]
[Route("api/auth")]
[RemoteService(Name = "Default")]
public class AuthController : PmsControllerBase
{
	private readonly IAuthAppService _authAppService;

	public AuthController(IAuthAppService authAppService)
	{
		_authAppService = authAppService;
	}

	/// <summary>
	/// 用户登录
	/// </summary>
	/// <param name="input">登录信息</param>
	/// <returns>令牌信息</returns>
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<ActionResult<LoginResponseDto>> LoginAsync([FromBody] LoginRequestDto input)
	{
		var result = await _authAppService.LoginAsync(input);
		return Ok(result);
	}

	/// <summary>
	/// 刷新令牌
	/// </summary>
	/// <param name="input">刷新令牌</param>
	/// <returns>新令牌</returns>
	[HttpPost("refresh")]
	[AllowAnonymous]
	public async Task<ActionResult<TokenResponseDto>> RefreshTokenAsync([FromBody] RefreshTokenRequestDto input)
	{
		var result = await _authAppService.RefreshTokenAsync(input);
		return Ok(result);
	}

	/// <summary>
	/// 登出（撤销令牌）
	/// </summary>
	[HttpPost("logout")]
	[Authorize]
	public async Task<ActionResult> LogoutAsync([FromBody] RevokeTokenRequestDto? input)
	{
		await _authAppService.RevokeTokenAsync(input ?? new RevokeTokenRequestDto());
		return Ok(new { message = "登出成功" });
	}

	/// <summary>
	/// 获取当前用户信息
	/// </summary>
	[HttpGet("current-user")]
	[Authorize]
	public async Task<ActionResult<UserInfoDto>> GetCurrentUserAsync()
	{
		var result = await _authAppService.GetCurrentUserAsync();
		return Ok(result);
	}

	/// <summary>
	/// 获取用户活跃会话列表
	/// </summary>
	[HttpGet("sessions")]
	[Authorize]
	public async Task<ActionResult<List<UserSessionDto>>> GetActiveSessionsAsync()
	{
		var result = await _authAppService.GetActiveSessionsAsync();
		return Ok(result);
	}

	/// <summary>
	/// 撤销指定会话
	/// </summary>
	[HttpDelete("sessions/{tokenId}")]
	[Authorize]
	public async Task<ActionResult> RevokeSessionAsync(Guid tokenId)
	{
		await _authAppService.RevokeSessionAsync(tokenId);
		return Ok(new { success = true, message = "会话已撤销" });
	}
}
