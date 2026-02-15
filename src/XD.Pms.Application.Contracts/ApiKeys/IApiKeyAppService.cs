using System;
using System.Threading.Tasks;
using XD.Pms.ApiKeys.Dto;
using XD.Pms.Services.Dtos;

namespace XD.Pms.ApiKeys;

public interface IApiKeyAppService
{
	/// <summary>
	/// 获取列表
	/// </summary>
	Task<PagedResponseDto<ApiKeyDto>> GetListAsync(ApiKeyListInput input);

	/// <summary>
	/// 获取详情
	/// </summary>
	Task<ApiKeyDto> GetAsync(Guid id);

	/// <summary>
	/// 创建（返回明文 Key）
	/// </summary>
	Task<CreateApiKeyOutput> CreateAsync(CreateApiKeyInput input);

	/// <summary>
	/// 更新
	/// </summary>
	Task<ApiKeyDto> UpdateAsync(Guid id, UpdateApiKeyInput input);

	/// <summary>
	/// 删除
	/// </summary>
	Task DeleteAsync(Guid id);

	/// <summary>
	/// 激活
	/// </summary>
	Task ActivateAsync(Guid id);

	/// <summary>
	/// 禁用
	/// </summary>
	Task DeactivateAsync(Guid id);

	/// <summary>
	/// 重新生成 Key（返回新的明文 Key）
	/// </summary>
	Task<RegenerateApiKeyOutput> RegenerateAsync(Guid id);
}