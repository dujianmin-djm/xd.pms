using System.Threading.Tasks;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Authentication;

public interface ITokenAppService
{
	/// <summary>
	/// 用户登录（Password Grant）
	/// </summary>
	Task<LoginResponseDto> LoginAsync(LoginRequestDto input);

	/// <summary>
	/// 刷新令牌
	/// </summary>
	Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input);

	/// <summary>
	/// 撤销令牌（登出）
	/// </summary>
	Task RevokeTokenAsync(string? token = null);

	/// <summary>
	/// 获取当前用户信息
	/// </summary>
	Task<UserInfoDto> GetCurrentUserInfoAsync();
}