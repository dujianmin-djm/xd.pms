using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace XD.Pms.Authentication;

/// <summary>
/// Token 黑名单服务（基于分布式缓存）
/// </summary>
public class TokenBlacklistService : ITokenBlacklistService, ITransientDependency
{
	private readonly IDistributedCache _cache;
	private const string BlacklistPrefix = "pms:token:blacklist:";

	public TokenBlacklistService(IDistributedCache cache)
	{
		_cache = cache;
	}

	public async Task AddToBlacklistAsync(string jti, DateTime expiration)
	{
		var key = $"{BlacklistPrefix}{jti}";
		var ttl = expiration - DateTime.UtcNow;

		if (ttl > TimeSpan.Zero)
		{
			await _cache.SetStringAsync(key, "1", new DistributedCacheEntryOptions
			{
				AbsoluteExpiration = expiration
			});
		}
	}

	public async Task<bool> IsBlacklistedAsync(string jti)
	{
		var key = $"{BlacklistPrefix}{jti}";
		var value = await _cache.GetStringAsync(key);
		return !string.IsNullOrEmpty(value);
	}
}