using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XD.Pms.BaseData.Positions.Dto;
using XD.Pms.Services;

namespace XD.Pms.BaseData.Positions;

public interface IPositionAppService :
	ICrudAppService<PositionDto, Guid, PositionReadDto, PositionCreateDto, PositionUpdateDto>,
	IWorkflowAppService<Guid>
{
	Task<List<PositionLookupDto>> GetLookupAsync(Guid? departmentId = null);
}
