using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Escrow.Api.Application.UserPanel.Commands.DeleteUser;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.FunctionalTests.UserDetails.Commands;

using static Testing;

public class DeleteUserDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoListId()
    {
        var command = new DeleteUserCommand(99);
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldDeleteTodoList()
    {
        var userId = await SendAsync(new CreateUserCommand
        {
            FullName = "New List"
        });

        await SendAsync(new DeleteUserCommand(userId));

        var list = await FindAsync<UserDetail>(userId);

        list.Should().BeNull();
    }
}
