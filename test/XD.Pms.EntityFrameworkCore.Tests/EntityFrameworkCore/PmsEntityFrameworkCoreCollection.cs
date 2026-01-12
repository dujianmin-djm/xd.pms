using Xunit;

namespace XD.Pms.EntityFrameworkCore;

[CollectionDefinition(PmsTestConsts.CollectionDefinitionName)]
public class PmsEntityFrameworkCoreCollection : ICollectionFixture<PmsEntityFrameworkCoreFixture>
{

}
