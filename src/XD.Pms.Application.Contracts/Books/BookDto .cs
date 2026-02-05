using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;

namespace XD.Pms.Books;

public class BookDto : AuditedEntityDto<Guid>
{
    public required string Name { get; set; }

    public int Type { get; set; }

    public DateTime PublishDate { get; set; }

    public float Price { get; set; }

	public string ConcurrencyStamp { get; set; } = string.Empty;

	public ExtraPropertyDictionary ExtraProperties { get; set; } = [];
}
