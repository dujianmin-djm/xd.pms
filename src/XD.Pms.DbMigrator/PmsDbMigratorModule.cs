using XD.Pms.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace XD.Pms.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(PmsEntityFrameworkCoreModule),
    typeof(PmsApplicationContractsModule)
)]
public class PmsDbMigratorModule : AbpModule
{
}
