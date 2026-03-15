using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Events;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Digital.Net.Core.Test.Services.Users;

public class UserServiceTest : UnitTest
{
    private readonly Mock<IDocumentService> _documentServiceMock = new();
    private readonly Mock<IAuditService> _auditServiceMock = new();

    private static DigitalContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DigitalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DigitalContext(options);
    }

    private UserService BuildUserService(DigitalContext context) => new(
        _documentServiceMock.Object,
        _auditServiceMock.Object,
        context
    );

    private static User BuildTestUser() => new()
    {
        Password = PasswordUtils.HashPassword("Password"),
        Username = "User",
        Email = "Email",
        Login = "Login"
    };

    [Test]
    public async Task UpdatePasswordAsync_Should_Return_Error_When_Invalid_Credentials()
    {
        await using var context = GetInMemoryContext();
        var user = BuildTestUser();
        var service = BuildUserService(context);

        var result = await service.UpdatePasswordAsync(user, "wrong_password", "NewPassword123!");
        await Assert.That(result.HasErrorOfType<InvalidCredentialsException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Return_Error_When_Password_Malformed()
    {
        await using var context = GetInMemoryContext();
        var user = BuildTestUser();
        var service = BuildUserService(context);

        var result = await service.UpdatePasswordAsync(user, "Password", "weak");
        await Assert.That(result.HasErrorOfType<PasswordMalformedException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Succeed()
    {
        await using var context = GetInMemoryContext();
        var user = BuildTestUser();
        var service = BuildUserService(context);

        var result = await service.UpdatePasswordAsync(user, "Password", "NewPassword123!");
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Register_Success_AuditEvent()
    {
        await using var context = GetInMemoryContext();
        var user = BuildTestUser();
        var service = BuildUserService(context);

        await service.UpdatePasswordAsync(user, "Password", "NewPassword123!");
        _auditServiceMock.Verify(
            a => a.RegisterEventAsync(
                UserEvents.UpdatePassword,
                EventState.Success,
                It.IsAny<Result>(),
                user.Id,
                null,
                null,
                null
            ),
            Times.Once
        );
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Register_Failed_AuditEvent_When_InvalidCredentials()
    {
        await using var context = GetInMemoryContext();
        var user = BuildTestUser();
        var service = BuildUserService(context);

        await service.UpdatePasswordAsync(user, "wrong_password", "NewPassword123!");
        _auditServiceMock.Verify(
            a => a.RegisterEventAsync(
                UserEvents.UpdatePassword,
                EventState.Failed,
                It.IsAny<Result>(),
                user.Id,
                null,
                null,
                null
            ),
            Times.Once
        );
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Register_Failed_AuditEvent_When_PasswordMalformed()
    {
        await using var context = GetInMemoryContext();
        var user = BuildTestUser();
        var service = BuildUserService(context);

        await service.UpdatePasswordAsync(user, "Password", "weak");
        _auditServiceMock.Verify(
            a => a.RegisterEventAsync(
                UserEvents.UpdatePassword,
                EventState.Failed,
                It.IsAny<Result>(),
                user.Id,
                null,
                null,
                null
            ),
            Times.Once
        );
    }

    [Test]
    public async Task UpdateAvatar_Should_Return_Error_When_File_Too_Heavy()
    {
        await using var context = GetInMemoryContext();
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(long.MaxValue);
        var service = BuildUserService(context);

        var result = await service.UpdateAvatar(BuildTestUser(), formFileMock.Object);
        await Assert.That(result.HasErrorOfType<TooHeavyException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Return_Error_When_Unsupported_Format()
    {
        await using var context = GetInMemoryContext();
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.ContentType).Returns("application/json");
        var service = BuildUserService(context);

        var result = await service.UpdateAvatar(BuildTestUser(), formFileMock.Object);
        await Assert.That(result.HasErrorOfType<UnsupportedFormatException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Succeed()
    {
        await using var context = GetInMemoryContext();
        var user = BuildTestUser();
        var service = BuildUserService(context);
        var document = new Document { Id = Guid.NewGuid(), FileName = "test", MimeType = "test" };
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(100);
        formFileMock.Setup(f => f.ContentType).Returns("image/jpeg");
        _documentServiceMock
            .Setup(d => d.SaveImageDocumentAsync(formFileMock.Object, user))
            .ReturnsAsync(new Result<Document> { Value = document });

        var result = await service.UpdateAvatar(user, formFileMock.Object);
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(user.AvatarId).IsNotNull();
    }
}