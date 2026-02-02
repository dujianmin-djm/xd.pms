//using AutoMapper;
//using XD.Pms.ApiKeys;
//using XD.Pms.ApiKeys.Dto;
//using XD.Pms.Books;

//namespace XD.Pms;

//public class PmsApplicationAutoMapperProfile : Profile
//{
//    public PmsApplicationAutoMapperProfile()
//    {
//        CreateMap<Book, BookDto>();
//        CreateMap<CreateUpdateBookDto, Book>();


//		CreateMap<ApiKey, ApiKeyDto>()
//			.ForMember(dest => dest.Roles, opt => opt.MapFrom(src => src.GetRoles()))
//			.ForMember(dest => dest.Permissions, opt => opt.MapFrom(src => src.GetPermissions()))
//			.ForMember(dest => dest.AllowedIpAddresses, opt => opt.MapFrom(src => src.GetAllowedIpAddresses()));
//	}
//}
