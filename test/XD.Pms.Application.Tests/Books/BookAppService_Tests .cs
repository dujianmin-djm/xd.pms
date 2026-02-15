using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using XD.Pms.Services.Dtos;
using Xunit;

namespace XD.Pms.Books;

public abstract class BookAppService_Tests<TStartupModule> : PmsApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IBookAppService _bookAppService;

    protected BookAppService_Tests()
    {
        _bookAppService = GetRequiredService<IBookAppService>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Books()
    {
        //Act
        var result = await _bookAppService.GetListAsync(
            new PagedRequestDto()
        );

        //Assert
        result.Total.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(b => b.Name == "1984");
    }

    [Fact]
    public async Task Should_Create_A_Valid_Book()
    {
        //Act
        var result = await _bookAppService.CreateAsync(
            new CreateUpdateBookDto
            {
                Name = "New test book 42",
                Price = 10,
                PublishDate = DateTime.Now,
                Type = 1
            }
        );

        //Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("New test book 42");
    }
    
    [Fact]
    public async Task Should_Not_Create_A_Book_Without_Name()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _bookAppService.CreateAsync(
                new CreateUpdateBookDto
                {
                    Name = "",
                    Price = 10,
                    PublishDate = DateTime.Now,
                    Type = 2
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }
}
