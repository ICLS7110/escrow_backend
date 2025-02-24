
using Escrow.Api.Application.Features.Commands;
using Escrow.Api.Domain.Entities;


namespace Escrow.Api.Application.FunctionalTests.UserDetails.Commands;

using static Testing;

public class DeleteUserDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidUserIdId()
    {
        var command = new DeleteUserCommand(99);
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeleteUserDetail()
    {
        var userId = await SendAsync(new CreateUserCommand
        {
            FullName = "New List"
        });

        await SendAsync(new DeleteUserCommand(1));

        var list = await FindAsync<User>(userId);

        list.Should().BeNull();
    }
}
