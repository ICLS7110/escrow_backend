using Volo.Abp.Modularity;

namespace EscrowAPI;

[DependsOn(
    typeof(EscrowAPIDomainModule),
    typeof(EscrowAPITestBaseModule)
)]
public class EscrowAPIDomainTestModule : AbpModule
{

}
