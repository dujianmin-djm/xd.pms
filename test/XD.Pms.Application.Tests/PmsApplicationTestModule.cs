using Volo.Abp.Modularity;

namespace XD.Pms;

[DependsOn(
    typeof(PmsApplicationModule),
    typeof(PmsDomainTestModule)
)]
public class PmsApplicationTestModule : AbpModule
{

}
