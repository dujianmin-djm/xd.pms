using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace XD.Pms.Books;

[Mapper]
[MapExtraProperties]
public partial class BookAndBookDtoMapper : TwoWayMapperBase<Book, BookDto>
{
	public override partial BookDto Map(Book source);
	public override partial void Map(Book source, BookDto destination);

	public override partial Book ReverseMap(BookDto source);
	public override partial void ReverseMap(BookDto source, Book destination);
}


[Mapper]
public partial class CreateUpdateBookMapper : MapperBase<CreateUpdateBookDto, Book>
{
	[MapperIgnoreTarget(nameof(Book.Id))]
	[MapperIgnoreTarget(nameof(Book.CreationTime))]
	[MapperIgnoreTarget(nameof(Book.CreatorId))]
	[MapperIgnoreTarget(nameof(Book.LastModifierId))]
	[MapperIgnoreTarget(nameof(Book.LastModificationTime))]
	[MapperIgnoreTarget(nameof(Book.ConcurrencyStamp))]
	public override partial Book Map(CreateUpdateBookDto source);

	[MapperIgnoreTarget(nameof(Book.Id))]
	[MapperIgnoreTarget(nameof(Book.CreationTime))]
	[MapperIgnoreTarget(nameof(Book.CreatorId))]
	[MapperIgnoreTarget(nameof(Book.LastModifierId))]
	[MapperIgnoreTarget(nameof(Book.LastModificationTime))]
	[MapperIgnoreTarget(nameof(Book.ConcurrencyStamp))]
	public override partial void Map(CreateUpdateBookDto source, Book destination);
}
