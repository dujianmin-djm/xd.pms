using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace XD.Pms.Books;

public class BookDataSeederContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Book, Guid> _bookRepository;

    public BookDataSeederContributor(IRepository<Book, Guid> bookRepository)
    {
        _bookRepository = bookRepository;
	}

	/// <summary>
	/// 当运行应用程序时，ABP框架会自动调用所有实现了 IDataSeedContributor 的类的 SeedAsync 方法。
	/// 如果使用ABP的数据库迁移功能（运行 DbMigrator 项目），数据种子会被执行。
	/// 或 IDataSeeder.SeedAsync 方法被手动调用时，数据种子也会被执行。
	/// </summary>
	public async Task SeedAsync(DataSeedContext context)
    {
        if (await _bookRepository.GetCountAsync() <= 0)
        {
            await _bookRepository.InsertAsync(
                new Book
                {
                    Name = "1984",
                    Type = 1,
                    PublishDate = new DateTime(1949, 6, 8),
                    Price = 19.84f
                },
                autoSave: true
            );

            await _bookRepository.InsertAsync(
                new Book
                {
                    Name = "The Hitchhiker's Guide to the Galaxy",
                    Type = 4,
                    PublishDate = new DateTime(1995, 9, 27),
                    Price = 42.0f
                },
                autoSave: true
            );
        }
    }
}
