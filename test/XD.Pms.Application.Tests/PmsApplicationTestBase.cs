using Volo.Abp.Modularity;

namespace XD.Pms;

public abstract class PmsApplicationTestBase<TStartupModule> : PmsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
