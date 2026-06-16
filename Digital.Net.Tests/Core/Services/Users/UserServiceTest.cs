using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Exceptions;
using Digital.Net.Lib.Files;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Moq;

namespace Digital.Net.Tests.Core.Services.Users;

public class UserServiceTest : DbServiceTest<DigitalContext>
{
    private Mock<IDocumentService> _documentServiceMock = null!;
    private UserService _service = null!;

    protected override Task OnInitializedAsync()
    {
        _documentServiceMock = new Mock<IDocumentService>();
        _service = new UserService(_documentServiceMock.Object, Context);
        return Task.CompletedTask;
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Return_Error_When_Invalid_Credentials()
    {
        var user = Context.BuildTestUser();

        var result = await _service.UpdatePasswordAsync(user, "wrong_password", "NewPassword123!");
        await Assert.That(result.HasErrorOfType<InvalidCredentialsException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Return_Error_When_Password_Malformed()
    {
        var user = Context.BuildTestUser();

        var result = await _service.UpdatePasswordAsync(user, TestUserFactory.TestUserPassword, "weak");
        await Assert.That(result.HasErrorOfType<PasswordMalformedException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Succeed()
    {
        var user = Context.BuildTestUser();

        var result = await _service.UpdatePasswordAsync(user, TestUserFactory.TestUserPassword, "NewPassword123!");
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task UpdateAvatar_Should_Return_Error_When_File_Too_Heavy()
    {
        var user = Context.BuildTestUser();

        var result = await _service.UpdateAvatar(
            user,
            Stream.Null,
            new FileDefinition { FileName = "avatar.jpg", MimeType = "image/jpeg", FileSize = long.MaxValue });
        await Assert.That(result.HasErrorOfType<TooHeavyException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Return_Error_When_Unsupported_Format()
    {
        var user = Context.BuildTestUser();

        var result = await _service.UpdateAvatar(
            user,
            Stream.Null,
            new FileDefinition { FileName = "avatar.json", MimeType = "application/json", FileSize = 100 });
        await Assert.That(result.HasErrorOfType<UnsupportedFormatException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Succeed()
    {
        var user = Context.BuildTestUser();
        var document = new Document
        {
            FileName = $"{Guid.NewGuid()}.jpg",
            MimeType = "image/jpeg",
            FileSize = 100
        };
        await Context.Documents.AddAsync(document);
        await Context.SaveChangesAsync();

        _documentServiceMock
            .Setup(d => d.SaveImageDocumentAsync(It.IsAny<Stream>(), It.IsAny<FileDefinition>(), user,
                It.IsAny<int?>()))
            .ReturnsAsync(new Result<Document> { Value = document });

        var result = await _service.UpdateAvatar(
            user,
            Stream.Null,
            new FileDefinition { FileName = "avatar.jpg", MimeType = "image/jpeg", FileSize = 100 });
        await Assert.That(result.HasError).IsFalse();
        await Assert.That(user.AvatarId).IsNotNull();
    }

    [Test]
    public async Task DeleteUserAsync_Should_Return_Error_When_Target_Is_Admin()
    {
        var admin = Context.BuildTestUser(new TestUserPayload { IsAdmin = true });

        var result = await _service.DeleteUserAsync(admin.Id);
        await Assert.That(result.HasErrorOfType<CannotDeleteAdminException>()).IsTrue();
    }

    [Test]
    public async Task UpdateUserStatusAsync_Should_Return_Error_When_Deactivating_Admin()
    {
        var admin = Context.BuildTestUser(new TestUserPayload { IsAdmin = true });

        var result = await _service.UpdateUserStatusAsync(admin.Id, false);
        await Assert.That(result.HasErrorOfType<CannotRevokeAdminException>()).IsTrue();
    }

    [Test]
    public async Task UpdateUserRoleAsync_Should_Return_Error_When_Demoting_Admin()
    {
        var admin = Context.BuildTestUser(new TestUserPayload { IsAdmin = true });

        var result = await _service.UpdateUserRoleAsync(admin.Id, false);
        await Assert.That(result.HasErrorOfType<CannotDemoteAdminException>()).IsTrue();
    }
}
