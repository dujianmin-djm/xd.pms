using Volo.Abp.Modularity;

namespace XD.Pms;

/* Inherit from this class for your domain layer tests. */
public abstract class PmsDomainTestBase<TStartupModule> : PmsTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
