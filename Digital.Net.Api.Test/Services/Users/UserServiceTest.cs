using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Utils;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Documents.Exceptions;
using Digital.Net.Api.Services.Users;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Digital.Net.Api.Test.Services.Users;

public class UserServiceTest : UnitTest
{
    private readonly Mock<IDocumentService> _documentServiceMock = new();

    private static DigitalContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DigitalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DigitalContext(options);
    }

    private UserService BuildUserService(DigitalContext context) => new(
        _documentServiceMock.Object,
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