using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.BackgroundJobs;
using Volo.Abp.DependencyInjection;

namespace XD.Pms.Authentication;

/// <summary>
/// 过期 Refresh Token 清理后台任务
/// </summary>
public class RefreshTokenCleanupBackgroundJob : AsyncBackgroundJob<RefreshTokenCleanupArgs>, ITransientDependency
{
	private readonly IRefreshTokenRepository _refreshTokenRepository;

	public RefreshTokenCleanupBackgroundJob(IRefreshTokenRepository refreshTokenRepository)
	{
		_refreshTokenRepository = refreshTokenRepository;
	}

	public override async Task ExecuteAsync(RefreshTokenCleanupArgs args)
	{
		var deletedCount = await _refreshTokenRepository.CleanupExpiredTokensAsync();
		Logger.LogInformation("已清理 {Count} 个过期的 Refresh Token", deletedCount);
	}
}


/// <summary>
/// 过期 Refresh Token 清理任务参数
/// </summary>
public class RefreshTokenCleanupArgs
{
	public string RefreshToken { get; set; } = default!;
	public bool ForceCleanAll { get; set; } = false;
}
