using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;
using XD.Pms.BaseData.Positions;
using XD.Pms.BaseData.Positions.Dto;

namespace XD.Pms.BaseData;

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PositionMapper : MapperBase<Position, PositionDto>
{
	public override partial PositionDto Map(Position source);

	public override partial void Map(Position source, PositionDto destination);
}

[Mapper(RequiredMappingStrategy = RequiredMappingStrategy.None)]
public partial class PositionToPositionLookupDtoMapper : MapperBase<Position, PositionLookupDto>
{
	public override partial PositionLookupDto Map(Position source);
	public override partial void Map(Position source, PositionLookupDto destination);
}