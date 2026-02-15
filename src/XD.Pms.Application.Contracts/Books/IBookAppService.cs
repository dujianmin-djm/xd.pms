using System;
using XD.Pms.Services;
using XD.Pms.Services.Dtos;

namespace XD.Pms.Books;

public interface IBookAppService : ICrudAppService<BookDto, Guid, PagedRequestDto, CreateUpdateBookDto> 
{

}