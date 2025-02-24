
namespace Escrow.Api.Application.FunctionalTests.UserDetails.Commands;

using Escrow.Api.Application.Features.Commands;
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
            VatId = "VAT123456"
            
        };

        var userId = await SendAsync(command);
        //userId.Should().BeGreaterThan(0);
        //await FluentActions.Invoking(() => SendAsync(command)).Should().ThrowAsync<ValidationException>();
    }

}
