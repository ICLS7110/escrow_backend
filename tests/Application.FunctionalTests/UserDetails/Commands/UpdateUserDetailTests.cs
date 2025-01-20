using Escrow.Api.Application.Common.Exceptions;
using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Escrow.Api.Application.UserPanel.Commands.UpdateUser;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.FunctionalTests.UserDetails.Commands;

using static Testing;

public class UpdateUserDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireValidTodoListId()
    {
        var command = new UpdateUserCommand { UserId = "0099", FullName = "New Title" };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    [Test]
    public async Task ShouldRequireUniqueTitle()
    {
        var userId = await SendAsync(new CreateUserCommand
        {
            FullName = "New List"
        });

        await SendAsync(new CreateUserCommand
        {
            FullName = "Other List"
        });

        var command = new UpdateUserCommand
        {
            Id = userId,
            FullName = "Other List"
        };

        (await FluentActions.Invoking(() =>
            SendAsync(command))
                .Should().ThrowAsync<ValidationException>().Where(ex => ex.Errors.ContainsKey("Title")))
                .And.Errors["Title"].Should().Contain("'Title' must be unique.");
    }

    [Test]
    public async Task ShouldUpdateTodoList()
    {
        var userId = await RunAsDefaultUserAsync();

        var listId = await SendAsync(new CreateUserCommand
        {
            FullName = "New List"
        });

        var command = new UpdateUserCommand
        {
            UserId = userId,
            FullName = "Updated List Title"
        };

        await SendAsync(command);

        var list = await FindAsync<UserDetail>(userId);

        list.Should().NotBeNull();
        list!.FullName.Should().Be(command.FullName);
        list.LastModifiedBy.Should().NotBeNull();
        list.LastModifiedBy.Should().Be(userId);
        //list.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
