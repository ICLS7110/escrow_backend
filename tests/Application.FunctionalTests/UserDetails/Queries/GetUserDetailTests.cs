
using Escrow.Api.Application.Features.Queries;
using Escrow.Api.Domain.Entities;


namespace Escrow.Api.Application.FunctionalTests.UserDetails.Queries;

using static Testing;

public class GetUserDetailTests : BaseTestFixture
{
    /*
    [Test]
    public async Task ShouldReturnPriorityLevels()
    {
        //await RunAsDefaultUserAsync();

        var query = new GetUserByIdQuery("1");

        var result = await SendAsync(query);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task ShouldReturnAllListsAndItems()
    {
        //await RunAsDefaultUserAsync();

        await AddAsync(new User
        {
            FullName = "Shopping",
            Gender = "Male"
        });

        var query = new GetUserDetailsQuery();

        var result = await SendAsync(query);

        result.Should().GetHashCode().Equals(1);
    }
    */
}
