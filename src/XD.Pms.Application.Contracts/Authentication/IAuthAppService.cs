using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using XD.Pms.Authentication.Dto;

namespace XD.Pms.Authentication;

public interface IAuthAppService : IApplicationService
{
	/// <summary>
	/// 用户登录
	/// </summary>
	Task<LoginResponseDto> LoginAsync(LoginRequestDto input);

	/// <summary>
	/// 刷新令牌
	/// </summary>
	Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenRequestDto input);

	/// <summary>
	/// 撤销令牌（登出）
	/// </summary>
	Task RevokeTokenAsync(RevokeTokenRequestDto input);

	/// <summary>
	/// 获取当前用户信息
	/// </summary>
	Task<UserInfoDto> GetCurrentUserAsync();

	/// <summary>
	/// 获取用户活跃会话列表
	/// </summary>
	Task<List<UserSessionDto>> GetActiveSessionsAsync();

	/// <summary>
	/// 撤销指定会话
	/// </summary>
	Task RevokeSessionAsync(System.Guid tokenId);
}
