using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Timing;
using Volo.Abp.Uow;

namespace XD.Pms.ApiKeys;

/// <summary>
/// API Key 验证器实现
/// </summary>
public class ApiKeyValidator : IApiKeyValidator, ITransientDependency
{
	private readonly IApiKeyRepository _apiKeyRepository;
	private readonly IDistributedCache<ApiKeyCacheItem> _cache;
	private readonly IClock _clock;
	private readonly ILogger<ApiKeyValidator> _logger;
	private readonly IUnitOfWorkManager _unitOfWorkManager;

	public ApiKeyValidator(
		IApiKeyRepository apiKeyRepository,
		IDistributedCache<ApiKeyCacheItem> cache,
		IClock clock,
		ILogger<ApiKeyValidator> logger,
		IUnitOfWorkManager unitOfWorkManager)
	{
		_apiKeyRepository = apiKeyRepository;
		_cache = cache;
		_clock = clock;
		_logger = logger;
		_unitOfWorkManager = unitOfWorkManager;
	}

	public async Task<ApiKeyValidationResult> ValidateAsync(string apiKey, string? ipAddress = null)
	{
		if (string.IsNullOrWhiteSpace(apiKey))
		{
			return ApiKeyValidationResult.Fail("API Key 不能为空");
		}

		var keyHash = ApiKeyManager.HashData(apiKey);
		var cacheKey = $"{ApiKeyConsts.CacheKeyPrefix}{keyHash}";

		// 尝试从缓存获取
		var cached = await _cache.GetAsync(cacheKey);

		if (cached != null)
		{
			return ProcessCachedResult(cached, ipAddress);
		}

		// 从数据库查询
		var entity = await _apiKeyRepository.FindByKeyHashAsync(keyHash);

		if (entity == null)
		{
			// 缓存无效结果，防止暴力破解
			await CacheInvalidResultAsync(cacheKey);

			_logger.LogWarning("无效的 API Key 尝试: {KeyPrefix}", apiKey[..Math.Min(8, apiKey.Length)]);

			return ApiKeyValidationResult.Fail("无效的 API Key");
		}

		// 验证状态
		var validationResult = ValidateEntity(entity, ipAddress);

		if (validationResult.IsValid)
		{
			// 缓存有效结果
			await CacheValidResultAsync(cacheKey, entity);

			// 异步更新使用信息
			_ = UpdateLastUsedAsync(entity.Id, ipAddress);
		}

		return validationResult;
	}

	private ApiKeyValidationResult ValidateEntity(ApiKey entity, string? ipAddress)
	{
		// 检查是否激活
		if (!entity.IsActive)
		{
			return ApiKeyValidationResult.Fail("API Key 已被禁用");
		}

		// 检查是否过期
		if (entity.IsExpired())
		{
			return ApiKeyValidationResult.Fail("API Key 已过期");
		}

		// 检查 IP 地址
		if (!entity.IsIpAllowed(ipAddress))
		{
			_logger.LogWarning(
				"API Key {ClientId} 的 IP 地址 {IpAddress} 不在允许列表中",
				entity.ClientId,
				ipAddress);

			return ApiKeyValidationResult.Fail("IP 地址不在允许列表中");
		}

		return ApiKeyValidationResult.Success(
			entity.Id,
			entity.ClientId,
			entity.ClientName,
			entity.UserId,
			entity.GetRoles(),
			entity.GetPermissions());
	}

	private static ApiKeyValidationResult ProcessCachedResult(ApiKeyCacheItem cached, string? ipAddress)
	{
		if (!cached.IsValid)
		{
			return ApiKeyValidationResult.Fail(cached.FailureMessage ?? "无效的 API Key");
		}

		// 检查 IP
		if (cached.AllowedIpAddresses?.Count > 0 && !string.IsNullOrEmpty(ipAddress))
		{
			if (!cached.AllowedIpAddresses.Contains(ipAddress))
			{
				return ApiKeyValidationResult.Fail("IP 地址不在允许列表中");
			}
		}

		return ApiKeyValidationResult.Success(
			cached.ApiKeyId!.Value,
			cached.ClientId!,
			cached.ClientName!,
			cached.UserId,
			cached.Roles,
			cached.Permissions);
	}

	private async Task CacheValidResultAsync(string cacheKey, ApiKey entity)
	{
		var cacheItem = new ApiKeyCacheItem
		{
			IsValid = true,
			ApiKeyId = entity.Id,
			ClientId = entity.ClientId,
			ClientName = entity.ClientName,
			UserId = entity.UserId,
			Roles = entity.GetRoles(),
			Permissions = entity.GetPermissions(),
			AllowedIpAddresses = entity.GetAllowedIpAddresses()
		};

		await _cache.SetAsync(
			cacheKey,
			cacheItem,
			new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ApiKeyConsts.CacheExpirationMinutes)
			});
	}

	private async Task CacheInvalidResultAsync(string cacheKey)
	{
		var cacheItem = new ApiKeyCacheItem
		{
			IsValid = false,
			FailureMessage = "Invaild API Key"
		};

		await _cache.SetAsync(
			cacheKey,
			cacheItem,
			new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(ApiKeyConsts.InvalidKeyCacheExpirationMinutes)
			});
	}

	private async Task UpdateLastUsedAsync(Guid id, string? ipAddress)
	{
		try
		{
			using var uow = _unitOfWorkManager.Begin(requiresNew: true, isTransactional: false);

			await _apiKeyRepository.UpdateLastUsedAsync(id, _clock.Now, ipAddress);

			await uow.CompleteAsync();
		}
		catch (Exception ex)
		{
			_logger.LogWarning(ex, "更新 API Key 最后使用时间失败: {Id}", id);
		}
	}
}

/// <summary>
/// API Key 缓存项
/// </summary>
public class ApiKeyCacheItem
{
	public bool IsValid { get; set; }
	public string? FailureMessage { get; set; }
	public Guid? ApiKeyId { get; set; }
	public string? ClientId { get; set; }
	public string? ClientName { get; set; }
	public Guid? UserId { get; set; }
	public List<string>? Roles { get; set; }
	public List<string>? Permissions { get; set; }
	public List<string>? AllowedIpAddresses { get; set; }
}