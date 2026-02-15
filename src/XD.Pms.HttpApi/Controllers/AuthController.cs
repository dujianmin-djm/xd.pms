using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using XD.Pms.ApiResponse;
using XD.Pms.Authentication;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Controllers;

[Route("papi/auth")]
public class AuthController : PmsControllerBase
{
	private readonly ITokenAppService _tokenAppService;
	private readonly ICryptoService _cryptoService;

	public AuthController(ITokenAppService tokenAppService, ICryptoService cryptoService)
	{
		_tokenAppService = tokenAppService;
		_cryptoService = cryptoService;
	}

	/// <summary>
	/// 获取 RSA 公钥
	/// </summary>
	[HttpGet("public-key")]
	[AllowAnonymous]
	public PublicKeyDto GetPublicKey()
	{
		var publicKey = _cryptoService.GetPublicKey();
		return new PublicKeyDto
		{
			PublicKey = publicKey,
			ExpiresAt = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeMilliseconds()
		};
	}

	/// <summary>
	/// 用户登录
	/// </summary>
	[HttpPost("login")]
	[AllowAnonymous]
	public async Task<ApiResponse<LoginResponseDto>> LoginAsync([FromBody] LoginRequestDto input)
	{
		if (input.IsEncrypted)
		{
			input.Password = _cryptoService.Decrypt(input.Password);
		}
		var result = await _tokenAppService.LoginAsync(input);
		return ApiResponse<LoginResponseDto>.Succeed(true, result, L["Auth:LoginSuccess"].Value);
	}

	/// <summary>
	/// 刷新令牌
	/// </summary>
	[HttpPost("refresh-token")]
	[AllowAnonymous]
	public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync([FromBody] RefreshTokenRequestDto input)
	{
		var result = await _tokenAppService.RefreshTokenAsync(input);
		return ApiResponse<LoginResponseDto>.Succeed(true, result, L["Auth:TokenRefreshSuccess"].Value);
	}

	/// <summary>
	/// 登出，撤销当前访问令牌
	/// </summary>
	[HttpPost("logout")]
	[Authorize]
	public async Task<ApiResponse<object>> LogoutAsync()
	{
		await _tokenAppService.RevokeTokenAsync();
		return ApiResponse<object>.Succeed(true, null, L["Auth:LogoutSuccess"].Value);
	}

	/// <summary>
	/// 撤销指定访问令牌
	/// </summary>
	[HttpPost("revoke-token")]
	[Authorize]
	public async Task<ApiResponse<object>> RevokeTokenAsync([FromBody] RevokeTokenRequestDto input)
	{
		await _tokenAppService.RevokeTokenAsync(input.AccessToken);
		return ApiResponse<object>.Succeed(true, null, L["Auth:TokenRevokeSuccess"].Value);
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
