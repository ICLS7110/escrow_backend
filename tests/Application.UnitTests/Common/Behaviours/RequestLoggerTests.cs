using Escrow.Api.Application.Common.Behaviours;
using Escrow.Api.Application.Common.Interfaces;
using Escrow.Api.Application.UserPanel.Commands.CreateUser;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;

namespace Escrow.Api.Application.UnitTests.Common.Behaviours;

public class RequestLoggerTests
{
    private Mock<ILogger<CreateUserCommand>> _logger = null!;
    private Mock<IUser> _user = null!;
    private Mock<IIdentityService> _identityService = null!;

    [SetUp]
    public void Setup()
    {
        _logger = new Mock<ILogger<CreateUserCommand>>();
        _user = new Mock<IUser>();
        _identityService = new Mock<IIdentityService>();
    }

    [Test]
    public async Task ShouldCallGetUserNameAsyncOnceIfAuthenticated()
    {
        _user.Setup(x => x.Id).Returns(Guid.NewGuid().ToString());

        var requestLogger = new LoggingBehaviour<CreateUserCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateUserCommand { UserId = 1, FullName = "title" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Once);
    }

    [Test]
    public async Task ShouldNotCallGetUserNameAsyncOnceIfUnauthenticated()
    {
        var requestLogger = new LoggingBehaviour<CreateUserCommand>(_logger.Object, _user.Object, _identityService.Object);

        await requestLogger.Process(new CreateUserCommand { UserId = 1, FullName = "title" }, new CancellationToken());

        _identityService.Verify(i => i.GetUserNameAsync(It.IsAny<string>()), Times.Never);
    }
}
