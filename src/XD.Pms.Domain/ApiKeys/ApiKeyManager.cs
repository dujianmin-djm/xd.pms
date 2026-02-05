using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using XD.Pms.ApiResponse;

namespace XD.Pms.ApiKeys;

/// <summary>
/// API Key 领域服务
/// </summary>
public class ApiKeyManager : DomainService
{
	private readonly IApiKeyRepository _apiKeyRepository;

	public ApiKeyManager(IApiKeyRepository apiKeyRepository)
	{
		_apiKeyRepository = apiKeyRepository;
	}

	/// <summary>
	/// 创建 API Key
	/// </summary>
	/// <returns>返回明文 Key 和实体</returns>
	public async Task<(string PlainKey, ApiKey Entity)> CreateAsync(
		string clientId,
		string clientName,
		string? description = null,
		DateTime? expiresAt = null,
		IEnumerable<string>? roles = null,
		IEnumerable<string>? permissions = null,
		IEnumerable<string>? allowedIpAddresses = null,
		int rateLimitPerMinute = 0,
		Guid? userId = null,
		string keyPrefix = "pk")
	{
		if (await _apiKeyRepository.ClientIdExistsAsync(clientId))
		{
			throw new BusinessException(ApiResponseCode.ValidationError, $"客户端 ID 【{clientId}】 已存在");
		}

		var plainKey = GenerateApiKey(keyPrefix);
		var keyHash = HashData(plainKey);
		var displayPrefix = plainKey[..Math.Min(12, plainKey.Length)] + "...";

		var apiKey = new ApiKey(
			GuidGenerator.Create(),
			keyHash,
			displayPrefix,
			clientId,
			clientName,
			userId);

		apiKey.SetDescription(description);
		apiKey.SetExpiresAt(expiresAt);
		apiKey.SetRoles(roles);
		apiKey.SetPermissions(permissions);
		apiKey.SetAllowedIpAddresses(allowedIpAddresses);
		apiKey.SetRateLimitPerMinute(rateLimitPerMinute);

		await _apiKeyRepository.InsertAsync(apiKey);

		return (plainKey, apiKey);
	}

	/// <summary>
	/// 更新 API Key
	/// </summary>
	public async Task<ApiKey> UpdateAsync(
		Guid id,
		string clientName,
		string? description = null,
		DateTime? expiresAt = null,
		IEnumerable<string>? roles = null,
		IEnumerable<string>? permissions = null,
		IEnumerable<string>? allowedIpAddresses = null,
		int rateLimitPerMinute = 0)
	{
		var apiKey = await _apiKeyRepository.GetAsync(id);

		apiKey.SetClientName(clientName);
		apiKey.SetDescription(description);
		apiKey.SetExpiresAt(expiresAt);
		apiKey.SetRoles(roles);
		apiKey.SetPermissions(permissions);
		apiKey.SetAllowedIpAddresses(allowedIpAddresses);
		apiKey.SetRateLimitPerMinute(rateLimitPerMinute);

		await _apiKeyRepository.UpdateAsync(apiKey);

		return apiKey;
	}

	/// <summary>
	/// 重新生成 API Key
	/// </summary>
	/// <returns>返回新的明文 Key</returns>
	public async Task<string> RegenerateAsync(Guid id, string keyPrefix = "pk")
	{
		var apiKey = await _apiKeyRepository.GetAsync(id);

		var plainKey = GenerateApiKey(keyPrefix);
		var keyHash = HashData(plainKey);
		var displayPrefix = plainKey[..Math.Min(12, plainKey.Length)] + "...";

		apiKey.SetKeyHash(keyHash);
		apiKey.SetKeyPrefix(displayPrefix);

		await _apiKeyRepository.UpdateAsync(apiKey);

		return plainKey;
	}

	/// <summary>
	/// 激活 API Key
	/// </summary>
	public async Task ActivateAsync(Guid id)
	{
		var apiKey = await _apiKeyRepository.GetAsync(id);
		apiKey.Activate();
		await _apiKeyRepository.UpdateAsync(apiKey);
	}

	/// <summary>
	/// 禁用 API Key
	/// </summary>
	public async Task DeactivateAsync(Guid id)
	{
		var apiKey = await _apiKeyRepository.GetAsync(id);
		apiKey.Deactivate();
		await _apiKeyRepository.UpdateAsync(apiKey);
	}

	/// <summary>
	/// 根据明文 Key 查找
	/// </summary>
	public async Task<ApiKey?> FindByPlainKeyAsync(string plainKey)
	{
		var keyHash = HashData(plainKey);
		return await _apiKeyRepository.FindByKeyHashAsync(keyHash);
	}

	/// <summary>
	/// 计算 Key 的哈希值
	/// </summary>
	public static string HashData(string plainKey)
	{
		var bytes = Encoding.UTF8.GetBytes(plainKey);
		var hash = SHA256.HashData(bytes);
		return Convert.ToBase64String(hash);
	}

	/// <summary>
	/// 生成随机 API Key
	/// </summary>
	private static string GenerateApiKey(string prefix)
	{
		var bytes = new byte[32];
		using var rng = RandomNumberGenerator.Create();
		rng.GetBytes(bytes);
		var base64 = Convert.ToBase64String(bytes).Replace("+", "").Replace("/", "").Replace("=", "");
		return $"{prefix}_{base64}";
	}
}