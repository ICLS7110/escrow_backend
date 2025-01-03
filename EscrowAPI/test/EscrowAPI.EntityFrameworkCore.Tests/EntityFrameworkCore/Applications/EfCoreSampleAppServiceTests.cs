using EscrowAPI.Samples;
using Xunit;

namespace EscrowAPI.EntityFrameworkCore.Applications;

[Collection(EscrowAPITestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<EscrowAPIEntityFrameworkCoreTestModule>
{

}
