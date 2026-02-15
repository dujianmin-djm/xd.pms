using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace XD.Pms.Services.Dtos;

[Serializable]
public class PagedResponseDto<T> : ListResultDto<T>
{
	public long Total { get; set; }
	public int Current { get; set; }
	public int Size { get; set; }
	public string? Sorts { get; set; }

	public PagedResponseDto(long totalCount, IReadOnlyList<T> items) : base(items)
	{
		Total = totalCount;
	}

	public PagedResponseDto(
		long totalCount,
		IReadOnlyList<T> items,
		int pageNumber, 
		int pageSize,
		string? sorts = null) 
		: base(items)
	{
		Total = totalCount;
		Current = pageNumber;
		Size = pageSize;
		Sorts = sorts;
	}
}
