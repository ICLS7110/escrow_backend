using EscrowAPI.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace EscrowAPI.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(EscrowAPIEntityFrameworkCoreModule),
    typeof(EscrowAPIApplicationContractsModule)
)]
public class EscrowAPIDbMigratorModule : AbpModule
{
}
