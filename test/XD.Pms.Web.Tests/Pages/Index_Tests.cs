using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace XD.Pms.Pages;

[Collection(PmsTestConsts.CollectionDefinitionName)]
public class Index_Tests : PmsWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
