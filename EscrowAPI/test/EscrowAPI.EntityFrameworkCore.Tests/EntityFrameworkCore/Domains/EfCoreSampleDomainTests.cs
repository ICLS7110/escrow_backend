using EscrowAPI.Samples;
using Xunit;

namespace EscrowAPI.EntityFrameworkCore.Domains;

[Collection(EscrowAPITestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<EscrowAPIEntityFrameworkCoreTestModule>
{

}
