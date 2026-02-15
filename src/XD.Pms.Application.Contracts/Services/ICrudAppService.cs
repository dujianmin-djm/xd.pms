using Volo.Abp;
using Volo.Abp.Application.Services;

namespace XD.Pms.Services;

public interface ICrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput> : 
	ICrudAppService<TEntityDto, TEntityDto, TKey, TGetListInput, TCreateInput, TUpdateInput>, 
	IReadOnlyAppService<TEntityDto, TEntityDto, TKey, TGetListInput>, 
	ICreateUpdateAppService<TEntityDto, TKey, TCreateInput, TUpdateInput>, 
	ICreateAppService<TEntityDto, TCreateInput>, 
	IUpdateAppService<TEntityDto, TKey, TUpdateInput>, 
	IDeleteAppService<TKey>,
	IApplicationService,
	IRemoteService
{
}

public interface ICrudAppService<TGetOutputDto, TGetListOutputDto, in TKey, in TGetListInput, in TCreateInput, in TUpdateInput> : 
	IReadOnlyAppService<TGetOutputDto, TGetListOutputDto, TKey, TGetListInput>, 
	ICreateUpdateAppService<TGetOutputDto, TKey, TCreateInput, TUpdateInput>, 
	ICreateAppService<TGetOutputDto, TCreateInput>, 
	IUpdateAppService<TGetOutputDto, TKey, TUpdateInput>, 
	IDeleteAppService<TKey>,
	IApplicationService, 
	IRemoteService
{
}

public interface ICrudAppService<TEntityDto, in TKey, in TGetListInput, in TCreateInput> : 
	ICrudAppService<TEntityDto, TKey, TGetListInput, TCreateInput, TCreateInput>, 
	ICrudAppService<TEntityDto, TEntityDto, TKey, TGetListInput, TCreateInput, TCreateInput>, 
	IReadOnlyAppService<TEntityDto, TEntityDto, TKey, TGetListInput>, 
	ICreateUpdateAppService<TEntityDto, TKey, TCreateInput, TCreateInput>, 
	ICreateAppService<TEntityDto, TCreateInput>, 
	IUpdateAppService<TEntityDto, TKey, TCreateInput>, 
	IDeleteAppService<TKey>,
	IApplicationService, 
	IRemoteService
{
}