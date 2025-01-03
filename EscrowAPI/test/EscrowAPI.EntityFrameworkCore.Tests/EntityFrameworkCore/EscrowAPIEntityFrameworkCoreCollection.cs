using Xunit;

namespace EscrowAPI.EntityFrameworkCore;

[CollectionDefinition(EscrowAPITestConsts.CollectionDefinitionName)]
public class EscrowAPIEntityFrameworkCoreCollection : ICollectionFixture<EscrowAPIEntityFrameworkCoreFixture>
{

}
