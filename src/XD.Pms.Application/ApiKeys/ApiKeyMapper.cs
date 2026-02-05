using Riok.Mapperly.Abstractions;
using System;
using System.Collections.Generic;
using Volo.Abp.Mapperly;
using XD.Pms.ApiKeys.Dto;

namespace XD.Pms.ApiKeys;

[Mapper]
public partial class ApiKeyToApiKeyDtoMapper : MapperBase<ApiKey, ApiKeyDto>
{
	[MapperIgnoreSource(nameof(ApiKey.KeyHash))]
	[MapperIgnoreSource(nameof(ApiKey.ConcurrencyStamp))]
	[MapperIgnoreSource(nameof(ApiKey.ExtraProperties))]
	public override partial ApiKeyDto Map(ApiKey source);

	[MapperIgnoreSource(nameof(ApiKey.KeyHash))]
	[MapperIgnoreSource(nameof(ApiKey.ConcurrencyStamp))]
	[MapperIgnoreSource(nameof(ApiKey.ExtraProperties))]
	public override partial void Map(ApiKey source, ApiKeyDto destination);

	public override void AfterMap(ApiKey source, ApiKeyDto target)
	{
		target.Roles = SplitToList(source.Roles);
		target.Permissions = SplitToList(source.Permissions);
		target.AllowedIpAddresses = SplitToList(source.AllowedIpAddresses);
	}

	private static List<string> SplitToList(string? value)
	{
		if (string.IsNullOrWhiteSpace(value))
		{
			return [];
		}
		return [.. value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)];
	}
}