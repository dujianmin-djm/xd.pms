using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using XD.Pms.ApiKeys.Dto;
using XD.Pms.Permissions;

namespace XD.Pms.ApiKeys;

[RemoteService(IsEnabled = false)]
[Authorize(PmsPermissions.ApiKeys.Default)]
public class ApiKeyAppService : PmsAppService, IApiKeyAppService
{
	private readonly IApiKeyRepository _apiKeyRepository;
	private readonly ApiKeyManager _apiKeyManager;

	public ApiKeyAppService(IApiKeyRepository apiKeyRepository, ApiKeyManager apiKeyManager)
	{
		_apiKeyRepository = apiKeyRepository;
		_apiKeyManager = apiKeyManager;
	}

	/// <summary>
	/// 获取列表
	/// </summary>
	public async Task<PagedResultDto<ApiKeyDto>> GetListAsync(ApiKeyListInput input)
	{
		var totalCount = await _apiKeyRepository.GetCountAsync(input.Filter, input.IsActive);

		var items = await _apiKeyRepository.GetListAsync(
			input.Filter,
			input.IsActive,
			null,
			input.Sorting,
			input.MaxResultCount,
			input.SkipCount);

		return new PagedResultDto<ApiKeyDto>(
			totalCount,
			ObjectMapper.Map<List<ApiKey>, List<ApiKeyDto>>(items));
	}

	/// <summary>
	/// 获取详情
	/// </summary>
	public async Task<ApiKeyDto> GetAsync(Guid id)
	{
		var apiKey = await _apiKeyRepository.GetAsync(id);
		return ObjectMapper.Map<ApiKey, ApiKeyDto>(apiKey);
	}

	/// <summary>
	/// 创建
	/// </summary>
	[Authorize(PmsPermissions.ApiKeys.Create)]
	public async Task<CreateApiKeyOutput> CreateAsync(CreateApiKeyInput input)
	{
		var (plainKey, entity) = await _apiKeyManager.CreateAsync(
			input.ClientId,
			input.ClientName,
			input.Description,
			input.ExpiresAt,
			input.Roles,
			input.Permissions,
			input.AllowedIpAddresses,
			input.RateLimitPerMinute,
			CurrentUser.Id,
			input.KeyPrefix);

		return new CreateApiKeyOutput
		{
			ApiKey = plainKey,
			KeyInfo = ObjectMapper.Map<ApiKey, ApiKeyDto>(entity)
		};
	}

	/// <summary>
	/// 更新
	/// </summary>
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public async Task<ApiKeyDto> UpdateAsync(Guid id, UpdateApiKeyInput input)
	{
		var entity = await _apiKeyManager.UpdateAsync(
			id,
			input.ClientName,
			input.Description,
			input.ExpiresAt,
			input.Roles,
			input.Permissions,
			input.AllowedIpAddresses,
			input.RateLimitPerMinute);

		return ObjectMapper.Map<ApiKey, ApiKeyDto>(entity);
	}

	/// <summary>
	/// 删除
	/// </summary>
	[Authorize(PmsPermissions.ApiKeys.Delete)]
	public async Task DeleteAsync(Guid id)
	{
		await _apiKeyRepository.DeleteAsync(id);
	}

	/// <summary>
	/// 激活
	/// </summary>
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public async Task ActivateAsync(Guid id)
	{
		await _apiKeyManager.ActivateAsync(id);
	}

	/// <summary>
	/// 禁用
	/// </summary>
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public async Task DeactivateAsync(Guid id)
	{
		await _apiKeyManager.DeactivateAsync(id);
	}

	/// <summary>
	/// 重新生成
	/// </summary>
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public async Task<RegenerateApiKeyOutput> RegenerateAsync(Guid id)
	{
		var plainKey = await _apiKeyManager.RegenerateAsync(id);
		var entity = await _apiKeyRepository.GetAsync(id);

		return new RegenerateApiKeyOutput
		{
			ApiKey = plainKey,
			KeyPrefix = entity.KeyPrefix
		};
	}
}