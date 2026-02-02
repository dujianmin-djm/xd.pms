using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace XD.Pms.ApiKeys;

public interface IApiKeyRepository : IRepository<ApiKey, Guid>
{
	/// <summary>
	/// 根据 Key 哈希值查找
	/// </summary>
	Task<ApiKey?> FindByKeyHashAsync(
		string keyHash,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 根据客户端 ID 查找
	/// </summary>
	Task<ApiKey?> FindByClientIdAsync(
		string clientId,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 检查客户端 ID 是否存在
	/// </summary>
	Task<bool> ClientIdExistsAsync(
		string clientId,
		Guid? excludeId = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 获取列表
	/// </summary>
	Task<List<ApiKey>> GetListAsync(
		string? filter = null,
		bool? isActive = null,
		Guid? userId = null,
		string? sorting = null,
		int maxResultCount = int.MaxValue,
		int skipCount = 0,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 获取数量
	/// </summary>
	Task<long> GetCountAsync(
		string? filter = null,
		bool? isActive = null,
		Guid? userId = null,
		CancellationToken cancellationToken = default);

	/// <summary>
	/// 更新最后使用信息
	/// </summary>
	Task UpdateLastUsedAsync(
		Guid id,
		DateTime lastUsedAt,
		string? lastUsedIp,
		CancellationToken cancellationToken = default);
}