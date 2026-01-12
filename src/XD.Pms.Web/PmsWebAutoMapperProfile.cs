using AutoMapper;
using XD.Pms.Books;

namespace XD.Pms.Web;

public class PmsWebAutoMapperProfile : Profile
{
    public PmsWebAutoMapperProfile()
    {
        CreateMap<BookDto, CreateUpdateBookDto>();
        
        //Define your object mappings here, for the Web project
    }
}
