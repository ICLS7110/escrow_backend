using Escrow.Api.Application.Common.Exceptions;
using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Escrow.Api.Domain.Entities;
using Escrow.Api.Domain.Entities.UserPanel;
using FluentValidation.Results;

namespace Escrow.Api.Application.FunctionalTests.UserDetails.Commands;

using static Testing;

public class CreateUserDetailTests : BaseTestFixture
{
    [Test]
    public async Task ShouldRequireMinimumFields()
    {
        var command = new CreateUserCommand
        {
            UserId = "6",
            FullName = "John Doe",
            EmailAddress = "johndoe@example.com",
            PhoneNumber = "1234567890",
            Gender = "Male",
            DateOfBirth = DateTime.UtcNow,
            BusinessManagerName = "Jane Doe",
            BusinessEmail = "janedoe@business.com",
            VatId = "VAT123456",
            AccountHolderName = "John Doe",
            IBANNumber = "DE89370400440532013000",
            BICCode = "DEUTDEFFXXX",
            LoginMethod = "Email"
        };

        var userId = await SendAsync(command);
        userId.Should().BeGreaterThan(0);
        //await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

    //[Test]
    //public async Task ShouldRequireUniqueTitle()//Test comment for Harshit review
    //{
    //    await SendAsync(new CreateUserCommand
    //    {
    //        UserId="6",
    //        FullName = "abc"
    //    });

    //    var command = new CreateUserCommand
    //    {
    //        UserId = "7",
    //        FullName = "abc"
    //    };

    //    await FluentActions.Invoking(() =>
    //        SendAsync(command)).Should().ThrowAsync<ValidationException>().Where( e => e.Errors.Any(error => error.Key=="FullName"));
    //}

    //[Test]
    //public async Task ShouldCreateUserDetail()
    //{
    //    var userId = await RunAsDefaultUserAsync();

    //    var command = new CreateUserCommand
    //    {
    //        FullName = "Tasks"
    //    };

    //    var id = await SendAsync(command);

    //    var list = await FindAsync<UserDetail>(id);

    //    list.Should().NotBeNull();
    //    list!.FullName.Should().Be(command.FullName);
    //    list.CreatedBy.Should().Be(userId);
    //   // list.CreatedDate.Should().BeCloseTo(DateTime.Now, TimeSpan.FromMilliseconds(10000));
    //}
}
