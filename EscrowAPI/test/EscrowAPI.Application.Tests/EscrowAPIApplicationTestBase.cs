using Volo.Abp.Modularity;

namespace EscrowAPI;

public abstract class EscrowAPIApplicationTestBase<TStartupModule> : EscrowAPITestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
