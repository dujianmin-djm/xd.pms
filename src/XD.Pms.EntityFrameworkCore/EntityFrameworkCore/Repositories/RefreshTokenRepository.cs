using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Timing;
using XD.Pms.Authentication;

namespace XD.Pms.EntityFrameworkCore.Repositories;

public class RefreshTokenRepository : EfCoreRepository<PmsDbContext, RefreshToken, Guid>, IRefreshTokenRepository
{
	private readonly IClock _clock;
	public RefreshTokenRepository(IDbContextProvider<PmsDbContext> dbContextProvider, IClock clock) : base(dbContextProvider)
	{
		_clock = clock;
	}

	public async Task<RefreshToken?> FindByTokenAsync(string token, CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();
		return await dbSet.FirstOrDefaultAsync(x => x.Token == token, cancellationToken);
	}

	public async Task<List<RefreshToken>> GetActiveTokensByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();
		return await dbSet
			.Where(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > _clock.Now)
			.OrderByDescending(x => x.CreationTime)
			.ToListAsync(cancellationToken);
	}

	public async Task RevokeAllByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var dbContext = await GetDbContextAsync();
		await dbContext.Database.ExecuteSqlInterpolatedAsync(
			$@"UPDATE AppRefreshTokens 
               SET IsRevoked = 1, RevokedAt = {_clock.Now} 
               WHERE UserId = {userId} AND IsRevoked = 0",
			cancellationToken);
	}

	public async Task RevokeByUserIdAndDeviceAsync(Guid userId, string deviceId, CancellationToken cancellationToken = default)
	{
		var dbContext = await GetDbContextAsync();
		await dbContext.Database.ExecuteSqlInterpolatedAsync(
			$@"UPDATE AppRefreshTokens 
               SET IsRevoked = 1, RevokedAt = {_clock.Now} 
               WHERE UserId = {userId} AND DeviceId = {deviceId} AND IsRevoked = 0",
			cancellationToken);
	}

	public async Task<int> CleanupExpiredTokensAsync(CancellationToken cancellationToken = default)
	{
		var dbContext = await GetDbContextAsync();
		var cutoffDate = _clock.Now.AddDays(-AuthenticationConsts.RefreshTokenRetentionDays);

		return await dbContext.Database.ExecuteSqlInterpolatedAsync(
			$@"DELETE FROM AppRefreshTokens WHERE ExpiresAt < {cutoffDate}",
			cancellationToken);
	}

	public async Task<int> GetActiveTokenCountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
	{
		var dbSet = await GetDbSetAsync();
		return await dbSet.CountAsync(x => x.UserId == userId && !x.IsRevoked && x.ExpiresAt > _clock.Now, cancellationToken);
	}
}
