using Digital.Net.Authentication.Exceptions;
using Digital.Net.Authentication.Services;
using Digital.Net.Core.Messages;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Entities.Models.Avatars;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Repositories;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.Http;
using Moq;

namespace Digital.Net.Core.Services.Test.Users;

public class UserServiceTest : UnitTest
{
    private readonly Mock<IDocumentService> _documentServiceMock = new();
    private readonly Mock<IRepository<User>> _userRepositoryMock = new();
    private readonly Mock<IRepository<Avatar>> _avatarRepositoryMock = new();

    private UserService BuildUserService() => new(
        _documentServiceMock.Object,
        _userRepositoryMock.Object,
        _avatarRepositoryMock.Object
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
        var user = BuildTestUser();
        var service = BuildUserService();

        var result = await service.UpdatePasswordAsync(user, "wrong_password", "NewPassword123!");
        await Assert.That(result.HasErrorOfType<InvalidCredentialsException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Return_Error_When_Password_Malformed()
    {
        var user = BuildTestUser();
        var service = BuildUserService();

        var result = await service.UpdatePasswordAsync(user, "Password", "weak");
        await Assert.That(result.HasErrorOfType<PasswordMalformedException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Succeed()
    {
        var user = BuildTestUser();
        var service = BuildUserService();

        var result = await service.UpdatePasswordAsync(user, "Password", "NewPassword123!");
        await Assert.That(result.HasError).IsFalse();
        _userRepositoryMock.Verify(r => r.Update(user), Times.Once);
    }

    [Test]
    public async Task UpdateAvatar_Should_Return_Error_When_File_Too_Heavy()
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.Length).Returns(long.MaxValue);
        var service = BuildUserService();

        var result = await service.UpdateAvatar(BuildTestUser(), formFileMock.Object);
        await Assert.That(result.HasErrorOfType<TooHeavyException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Return_Error_When_Unsupported_Format()
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(f => f.ContentType).Returns("application/json");
        var service = BuildUserService();

        var result = await service.UpdateAvatar(BuildTestUser(), formFileMock.Object);
        await Assert.That(result.HasErrorOfType<UnsupportedFormatException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Succeed()
    {
        var user = BuildTestUser();
        var service = BuildUserService();
        var document = new Document { Id = Guid.NewGuid() };
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