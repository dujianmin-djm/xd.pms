using XD.Pms.Samples;
using Xunit;

namespace XD.Pms.EntityFrameworkCore.Domains;

[Collection(PmsTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<PmsEntityFrameworkCoreTestModule>
{

}
