using EscrowAPI.Books;
using Xunit;

namespace EscrowAPI.EntityFrameworkCore.Applications.Books;

[Collection(EscrowAPITestConsts.CollectionDefinitionName)]
public class EfCoreBookAppService_Tests : BookAppService_Tests<EscrowAPIEntityFrameworkCoreTestModule>
{

}