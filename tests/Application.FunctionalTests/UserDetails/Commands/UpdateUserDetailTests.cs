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
    public async Task ShouldRequireValidUserId()
    {
        var command = new UpdateUserCommand { UserId = "0099", FullName = "New Title" };
        await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<NotFoundException>();
    }

    //[Test]
    //public async Task ShouldRequireUniqueTitle()
    //{
    //    var userId = await SendAsync(new CreateUserCommand
    //    {
    //        FullName = "New List"
    //    });

    //    await SendAsync(new CreateUserCommand
    //    {
    //        FullName = "Other List"
    //    });

    //    var command = new UpdateUserCommand
    //    {
    //        Id = userId,
    //        FullName = "Other List"
    //    };

    //    (await FluentActions.Invoking(() =>
    //        SendAsync(command))
    //            .Should().ThrowAsync<ValidationException>().Where(ex => ex.Errors.ContainsKey("Title")))
    //            .And.Errors["Title"].Should().Contain("'Title' must be unique.");
    //}

    [Test]
    public async Task ShouldUpdateUserDetail()
    {
        var userId = "8";//await RunAsDefaultUserAsync();

        var listId = await SendAsync(new CreateUserCommand
        {
            UserId = "8",
            FullName = "John",
            EmailAddress = "john@example.com",
            PhoneNumber = "1234567866",
            Gender = "Male",
            DateOfBirth = DateTime.UtcNow,
            BusinessManagerName = "Jane",
            BusinessEmail = "jane@business.com",
            VatId = "VAT123458",
            AccountHolderName = "John",
            IBANNumber = "DE89370400440532014000",
            BICCode = "DEUTDEFFYYY",
            LoginMethod = "Email"
        });

        var command = new UpdateUserCommand
        {
            Id = listId,
            UserId = userId,
            FullName = "Updated List Title 5",
            EmailAddress = "john@example.com",
            Gender = "Male",
            DateOfBirth = DateTime.UtcNow,
            BusinessManagerName = "Jane",
            BusinessEmail = "jane@business.com",
            VatId = "VAT123458",
            AccountHolderName = "John",
            IBANNumber = "DE89370400440532014000",
            BICCode = "DEUTDEFFYYY",
            LoginMethod = "Email"
        };

        await SendAsync(command);

        var list = await FindAsync<UserDetail>(listId);

        list.Should().NotBeNull();
        list!.FullName.Should().Be(command.FullName);
        //list.LastModifiedBy.Should().NotBeNull();
        //list.LastModifiedBy.Should().Be(userId);
        //list.LastModifiedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    }
}
