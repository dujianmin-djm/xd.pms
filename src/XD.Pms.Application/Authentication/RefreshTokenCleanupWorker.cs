using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.BackgroundWorkers;
using Volo.Abp.Threading;

namespace XD.Pms.Authentication;

/// <summary>
/// 定期清理 过期的 Refresh Token
/// </summary>
public class RefreshTokenCleanupWorker : AsyncPeriodicBackgroundWorkerBase
{
	public RefreshTokenCleanupWorker(AbpAsyncTimer timer, IServiceScopeFactory serviceScopeFactory)
		: base(timer, serviceScopeFactory)
	{
		Timer.Period = 24 * 60 * 60 * 1000; // 每24小时执行一次
	}

	protected override async Task DoWorkAsync(PeriodicBackgroundWorkerContext workerContext)
	{
		Logger.LogInformation("Starting RefreshToken cleanup...");

		var repository = workerContext.ServiceProvider.GetRequiredService<IRefreshTokenRepository>();
		var deletedCount = await repository.CleanupExpiredTokensAsync();

		Logger.LogInformation("Cleaned up {Count} expired refresh tokens", deletedCount);
	}
}
