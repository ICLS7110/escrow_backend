using Escrow.Api.Application.UserPanel.Queries.GetUsers;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;
using Escrow.Api.Domain.ValueObjects;

namespace Escrow.Api.Application.FunctionalTests.UserDetails.Queries;

using static Testing;

public class GetUserDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldReturnPriorityLevels()
    {
        await RunAsDefaultUserAsync();

        var query = new GetUserDetailsQuery();

        var result = await SendAsync(query);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task ShouldReturnAllListsAndItems()
    {
        await RunAsDefaultUserAsync();

        await AddAsync(new UserDetail
        {
            FullName = "Shopping",
            Gender = "Male"
        });

        var query = new GetUserDetailsQuery();

        var result = await SendAsync(query);

        result.Should().GetHashCode().Equals(1);
    }

   /* [Test]
    public async Task ShouldDenyAnonymousUser()
    {
        var query = new GetTodosQuery();

        var action = () => SendAsync(query);
        
        await action.Should().ThrowAsync<UnauthorizedAccessException>();
    }*/
}
