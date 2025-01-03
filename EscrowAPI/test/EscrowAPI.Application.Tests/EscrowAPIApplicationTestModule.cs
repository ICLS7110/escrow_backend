using Volo.Abp.Modularity;

namespace EscrowAPI;

[DependsOn(
    typeof(EscrowAPIApplicationModule),
    typeof(EscrowAPIDomainTestModule)
)]
public class EscrowAPIApplicationTestModule : AbpModule
{

}
