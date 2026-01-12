using XD.Pms.Samples;
using Xunit;

namespace XD.Pms.EntityFrameworkCore.Applications;

[Collection(PmsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<PmsEntityFrameworkCoreTestModule>
{

}
