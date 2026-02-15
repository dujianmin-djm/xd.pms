using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using XD.Pms.Services.Dtos;

namespace XD.Pms.Services;

public interface IReadOnlyAppService<TGetOutputDto, TGetListOutputDto, in TKey, in TGetListInput> : IApplicationService, IRemoteService
{
	Task<TGetOutputDto> GetAsync(TKey id);
	Task<PagedResponseDto<TGetListOutputDto>> GetListAsync(TGetListInput input);
}
