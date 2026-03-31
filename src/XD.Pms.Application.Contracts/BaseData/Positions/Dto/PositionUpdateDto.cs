using Volo.Abp.Domain.Entities;

namespace XD.Pms.BaseData.Positions.Dto;

public class PositionUpdateDto : PositionCreateDto, IHasConcurrencyStamp
{
	public string ConcurrencyStamp { get; set; } = default!;
}
