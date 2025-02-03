using Escrow.Api.Application.Common.Exceptions;
using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;

namespace Escrow.Api.Application.FunctionalTests.UserDetails.Commands;

using static Testing;

public class CreateUserDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new CreateUserCommand();
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldRequireUniqueTitle()
    {
        await SendAsync(new CreateUserCommand
        {
            FullName = "abc"
        });

        var command = new CreateUserCommand
        {
            FullName = "abc"
        };

        await FluentActions.Invoking(() =>
            SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    [Test]
    public async Task ShouldCreateUserDetail()
    {
        var userId = await RunAsDefaultUserAsync();

        var command = new CreateUserCommand
        {
            FullName = "Tasks"
        };

        var id = await SendAsync(command);

        var list = await FindAsync<UserDetail>(id);

        list.Should().NotBeNull();
        list!.FullName.Should().Be(command.FullName);
        list.CreatedBy.Should().Be(userId);
       // list.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
