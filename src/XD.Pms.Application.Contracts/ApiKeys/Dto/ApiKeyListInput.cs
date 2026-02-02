using Volo.Abp.Application.Dtos;

namespace XD.Pms.ApiKeys.Dto;

public class ApiKeyListInput : PagedAndSortedResultRequestDto
{
	/// <summary>
	/// 搜索关键字（客户端ID、名称、描述）
	/// </summary>
	public string? Filter { get; set; }

	/// <summary>
	/// 是否激活
	/// </summary>
	public bool? IsActive { get; set; }
}