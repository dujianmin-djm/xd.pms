using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using Volo.Abp.Domain.Services;
using Volo.Abp.Features;
using Volo.Abp.Identity;
using Volo.Abp;
using Volo.Abp.DistributedLocking;
using XD.Pms.Localization;
using XD.Pms.Features;

namespace XD.Pms.Identity;

public class IdentityUserLimitManager : DomainService
{
	private readonly IIdentityUserRepository _userRepository;
	private readonly IStringLocalizer<PmsResource> _localizer;
	private readonly IFeatureChecker _featureChecker;
	private readonly IAbpDistributedLock _distributedLock;

	public IdentityUserLimitManager(
		IIdentityUserRepository userRepository,
		IStringLocalizer<PmsResource> localizer,
		IFeatureChecker featureChecker,
		IAbpDistributedLock distributedLock)
	{
		_userRepository = userRepository;
		_localizer = localizer;
		_featureChecker = featureChecker;
		_distributedLock = distributedLock;
	}

	public async Task<(bool IsExceed, int MaxUserCount)> CheckUserLimitIsExceedAsync()
	{
		var maxUserCountValue = await _featureChecker.GetOrNullAsync(PmsFeatures.MaxUserCount);
		if (!int.TryParse(maxUserCountValue, out int maxUserCount))
		{
			maxUserCount = 40;
		}
		var enabledUserCount = await _userRepository.GetCountAsync(notActive: false);
		if (enabledUserCount > maxUserCount)
		{
			return (IsExceed: true, MaxUserCount: maxUserCount);
		}
		return (IsExceed: false, 0);
	}

	public async Task CheckUserLimitAsync()
	{
		var maxUserCountValue = await _featureChecker.GetOrNullAsync(PmsFeatures.MaxUserCount);
		if (!int.TryParse(maxUserCountValue, out int maxUserCount))
		{
			maxUserCount = 21;
		}
		var enabledUserCount = await _userRepository.GetCountAsync(notActive: false);
		if (enabledUserCount >= maxUserCount)
		{
			throw new UserFriendlyException(
				code: "USER_LIMIT_EXCEEDED",
				message: _localizer["Tip:UserLimitExceeded"],
				details: _localizer["Tip:UserLimitExceededMessage", maxUserCount]
			);
		}
	}

	public async Task CheckUserLimitWithLockAsync()
	{
		await using var handle = await _distributedLock.TryAcquireAsync(nameof(IdentityUserLimitManager));
		Logger.LogDebug($"Lock is acquired for IdentityUserLimitManager.");
		if (handle != null)
		{
			await CheckUserLimitAsync();
		}
		else
		{
			throw new UserFriendlyException(_localizer["Tip:SystemBusyTryAgain"]);
		}
		Logger.LogDebug($"Lock is released for IdentityUserLimitManager.");
	}
}
