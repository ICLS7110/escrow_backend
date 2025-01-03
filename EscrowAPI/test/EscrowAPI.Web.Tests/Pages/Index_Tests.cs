using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace EscrowAPI.Pages;

[Collection(EscrowAPITestConsts.CollectionDefinitionName)]
public class Index_Tests : EscrowAPIWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
