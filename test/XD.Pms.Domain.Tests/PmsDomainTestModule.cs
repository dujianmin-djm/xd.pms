using Volo.Abp.Modularity;

namespace XD.Pms;

[DependsOn(
    typeof(PmsDomainModule),
    typeof(PmsTestBaseModule)
)]
public class PmsDomainTestModule : AbpModule
{

}
