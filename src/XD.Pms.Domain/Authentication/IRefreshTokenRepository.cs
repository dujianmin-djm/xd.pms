using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace XD.Pms.Authentication;

public interface IRefreshTokenRepository : IRepository<RefreshToken, Guid>
{
	/// <summary>
	/// 根据Token值查找
	/// </summary>
	Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default);

	/// <summary>
	/// 获取用户的所有活跃Token
	/// </summary>
	Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// 撤销用户的所有Token
	/// </summary>
	Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

	/// <summary>
	/// 撤销用户指定设备的所有Token
	/// </summary>
	Task RevokeByUserIdAndDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default);

	/// <summary>
	/// 清理过期Token
	/// </summary>
	Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default);

	/// <summary>
	/// 获取用户活跃Token数量
	/// </summary>
	Task<int> GetActiveTokenCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
}
