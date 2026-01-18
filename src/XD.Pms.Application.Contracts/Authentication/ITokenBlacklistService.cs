using System;
using System.Threading.Tasks;

namespace XD.Pms.Authentication;

/// <summary>
/// Token 黑名单服务接口
/// </summary>
public interface ITokenBlacklistService
{
	/// <summary>
	/// 将 Token 加入黑名单
	/// </summary>
	/// <param name="jti">Token 的 JTI (JWT ID)</param>
	/// <param name="expiration">Token 的过期时间</param>
	Task AddToBlacklistAsync(string jti, DateTime expiration);

	/// <summary>
	/// 检查 Token 是否在黑名单中
	/// </summary>
	Task<bool> IsBlacklistedAsync(string jti);
}