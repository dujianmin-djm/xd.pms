using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Controllers;

[Area("app")]
[Route("papi/auth")]
public class AuthController(ITokenAppService tokenAppService) : PmsControllerBase
{
	private readonly ITokenAppService _tokenAppService = tokenAppService;

	/// <summary>
	/// 用户登录
	/// </summary>
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginAsync([FromBody] LoginRequestDto input)
	{
		var result = await _tokenAppService.LoginAsync(input);
		return Ok(ApiResponse<LoginResponseDto>.Succeed(true, result, L["Auth:LoginSuccess"].Value));
	}

	/// <summary>
	/// 刷新令牌
	/// </summary>
	[HttpPost("refresh-token")]
	[AllowAnonymous]
	public async Task<ActionResult<ApiResponse<LoginResponseDto>>> RefreshTokenAsync([FromBody] RefreshTokenRequestDto input)
	{
		var result = await _tokenAppService.RefreshTokenAsync(input);
		return Ok(ApiResponse<LoginResponseDto>.Succeed(true, result, L["Auth:TokenRefreshSuccess"].Value));
	}

	/// <summary>
	/// 登出，撤销当前访问令牌
	/// </summary>
	[HttpPost("logout")]
	[Authorize]
	public async Task<ActionResult<ApiResponse<object>>> LogoutAsync()
	{
		await _tokenAppService.RevokeTokenAsync();
		return Ok(ApiResponse<object>.Succeed(true, null, L["Auth:LogoutSuccess"].Value));
	}

	/// <summary>
	/// 撤销指定访问令牌
	/// </summary>
	[HttpPost("revoke-token")]
	[Authorize]
	public async Task<ActionResult<ApiResponse<object>>> RevokeTokenAsync([FromBody] RevokeTokenRequestDto input)
	{
		await _tokenAppService.RevokeTokenAsync(input.AccessToken);
		return Ok(ApiResponse<object>.Succeed(true, null, L["Auth:TokenRevokeSuccess"].Value));
	}

	/// <summary>
	/// 获取当前用户信息
	/// </summary>
	[HttpGet("user-info")]
	[Authorize]
	public async Task<UserInfoDto> GetUserInfoAsync()
	{
		return await _tokenAppService.GetCurrentUserInfoAsync();
	}
}
