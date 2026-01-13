using Riok.Mapperly.Abstractions;
using Volo.Abp.Mapperly;

namespace XD.Pms.Books;

[Mapper]
[MapExtraProperties]
public partial class BookMapper : TwoWayMapperBase<Book, BookDto>
{
	public override partial BookDto Map(Book source);
	public override partial void Map(Book source, BookDto destination);

	public override partial Book ReverseMap(BookDto source);
	public override partial void ReverseMap(BookDto source, Book destination);
}


[Mapper]
public partial class BookMapper2 : MapperBase<CreateUpdateBookDto, Book>
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


[Mapper]
public partial class BookMapper3 : MapperBase<BookDto, CreateUpdateBookDto>
{
	public override CreateUpdateBookDto Map(BookDto source)
	{
		var destination = new CreateUpdateBookDto
		{
			Name = source.Name,
			Type = source.Type,
			PublishDate = source.PublishDate,
			Price = source.Price
		};
		return destination;
	}

	public override void Map(BookDto source, CreateUpdateBookDto destination)
	{
		var mapped = Map(source);
		destination.Name = mapped.Name;
		destination.Type = mapped.Type;
		destination.PublishDate = mapped.PublishDate;
		destination.Price = mapped.Price;
	}
}