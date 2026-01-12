using XD.Pms.Books;
using Xunit;

namespace XD.Pms.EntityFrameworkCore.Applications.Books;

[Collection(PmsTestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<PmsEntityFrameworkCoreTestModule>
{

}
