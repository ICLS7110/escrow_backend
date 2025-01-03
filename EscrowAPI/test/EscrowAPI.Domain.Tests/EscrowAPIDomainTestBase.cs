using Volo.Abp.Modularity;

namespace EscrowAPI;

/* Inherit from this class for your domain layer tests. */
public abstract class EscrowAPIDomainTestBase<TStartupModule> : EscrowAPITestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
