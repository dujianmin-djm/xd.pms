using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using XD.Pms.ApiKeys;
using XD.Pms.ApiKeys.Dto;
using XD.Pms.Permissions;

namespace XD.Pms.Controllers;

[Area("app")]
[Route("papi/api-keys")]
[Authorize(PmsPermissions.ApiKeys.Default)]
public class ApiKeyController : PmsControllerBase, IApiKeyAppService
{
	private readonly IApiKeyAppService _apiKeyAppService;

	public ApiKeyController(IApiKeyAppService apiKeyAppService)
	{
		_apiKeyAppService = apiKeyAppService;
	}

	/// <summary>
	/// 获取 API Key 列表
	/// </summary>
	[HttpGet]
	public Task<PagedResultDto<ApiKeyDto>> GetListAsync([FromQuery] ApiKeyListInput input)
	{
		return _apiKeyAppService.GetListAsync(input);
	}

	/// <summary>
	/// 获取 API Key 详情
	/// </summary>
	[HttpGet("{id}")]
	public Task<ApiKeyDto> GetAsync(Guid id)
	{
		return _apiKeyAppService.GetAsync(id);
	}

	/// <summary>
	/// 创建 API Key
	/// </summary>
	/// <remarks>
	/// 创建成功后会返回明文 API Key，请妥善保存，此 Key 只会显示一次！
	/// </remarks>
	[HttpPost]
	[Authorize(PmsPermissions.ApiKeys.Create)]
	public Task<CreateApiKeyOutput> CreateAsync([FromBody] CreateApiKeyInput input)
	{
		return _apiKeyAppService.CreateAsync(input);
	}

	/// <summary>
	/// 更新 API Key
	/// </summary>
	[HttpPut("{id}")]
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public Task<ApiKeyDto> UpdateAsync(Guid id, [FromBody] UpdateApiKeyInput input)
	{
		return _apiKeyAppService.UpdateAsync(id, input);
	}

	/// <summary>
	/// 删除 API Key
	/// </summary>
	[HttpDelete("{id}")]
	[Authorize(PmsPermissions.ApiKeys.Delete)]
	public Task DeleteAsync(Guid id)
	{
		return _apiKeyAppService.DeleteAsync(id);
	}

	/// <summary>
	/// 激活 API Key
	/// </summary>
	[HttpPost("{id}/activate")]
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public Task ActivateAsync(Guid id)
	{
		return _apiKeyAppService.ActivateAsync(id);
	}

	/// <summary>
	/// 禁用 API Key
	/// </summary>
	[HttpPost("{id}/deactivate")]
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public Task DeactivateAsync(Guid id)
	{
		return _apiKeyAppService.DeactivateAsync(id);
	}

	/// <summary>
	/// 重新生成 API Key
	/// </summary>
	/// <remarks>
	/// 重新生成后旧的 Key 将立即失效，新 Key 只会显示一次！
	/// </remarks>
	[HttpPost("{id}/regenerate")]
	[Authorize(PmsPermissions.ApiKeys.Edit)]
	public Task<RegenerateApiKeyOutput> RegenerateAsync(Guid id)
	{
		return _apiKeyAppService.RegenerateAsync(id);
	}
}