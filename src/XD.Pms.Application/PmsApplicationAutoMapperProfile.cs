using AutoMapper;
using XD.Pms.Books;

namespace XD.Pms;

public class PmsApplicationAutoMapperProfile : Profile
{
    public PmsApplicationAutoMapperProfile()
    {
        CreateMap<Book, BookDto>();
        CreateMap<CreateUpdateBookDto, Book>();
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */
    }
}
