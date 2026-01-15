using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Controllers;

[Area("app")]
[Route("api/auth")]
[RemoteService(Name = "Default")]
public class AuthController : PmsControllerBase
{
	private readonly ITokenAppService _tokenAppService;

	public AuthController(ITokenAppService tokenAppService)
	{
		_tokenAppService = tokenAppService;
	}

	/// <summary>
	/// 用户登录
	/// </summary>
	/// <remarks>
	/// 使用用户名/邮箱和密码登录，获取访问令牌和刷新令牌。示例请求:
	/// {
	///     "userName": "admin",
	///     "password": "1q2w3E*"
	/// }
	/// </remarks>
	/// <param name="input">登录信息</param>
	/// <returns>令牌信息和用户信息</returns>
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<ActionResult<ApiResponse<LoginResponseDto>>> LoginAsync([FromBody] LoginRequestDto input)
	{
		var result = await _tokenAppService.LoginAsync(input);
		return Ok(ApiResponse<LoginResponseDto>.Success(result, "登录成功"));
	}

	/// <summary>
	/// 刷新令牌
	/// </summary>
	/// <remarks>
	/// 使用刷新令牌获取新的访问令牌。示例请求:
	/// {
	///     "refreshToken": "your-refresh-token"
	/// }
	/// </remarks>
	/// <param name="input">刷新令牌</param>
	/// <returns>新的令牌信息</returns>
	[HttpPost("refresh")]
	[AllowAnonymous]
	public async Task<ApiResponse<TokenResponseDto>> RefreshTokenAsync([FromBody] RefreshTokenRequestDto input)
	{
		var result = await _tokenAppService.RefreshTokenAsync(input);
		return ApiResponse<TokenResponseDto>.Success(result, "令牌刷新成功");
	}

	/// <summary>
	/// 登出（撤销令牌），撤销当前访问令牌，使其失效
	/// </summary>
	[HttpPost("logout")]
	[Authorize]
	public async Task<ApiResponse<object>> LogoutAsync()
	{
		await _tokenAppService.RevokeTokenAsync();
		return ApiResponse<object>.Success(null, "登出成功");
	}

	/// <summary>
	/// 获取当前用户信息
	/// </summary>
	[HttpGet("user-info")]
	[Authorize]
	public async Task<ApiResponse<UserInfoDto>> GetUserInfoAsync()
	{
		var result = await _tokenAppService.GetCurrentUserInfoAsync();
		return ApiResponse<UserInfoDto>.Success(result);
	}
}
