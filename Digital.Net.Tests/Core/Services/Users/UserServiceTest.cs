using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Users;
using Digital.Net.Core.Services.Users.Events;
using Digital.Net.Core.Services.Users.Exceptions;
using Digital.Net.Lib.Files;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Moq;
using TUnit.Core.Interfaces;

namespace Digital.Net.Tests.Core.Services.Users;

public class UserServiceTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private DigitalContext _context = null!;
    private Mock<IDocumentService> _documentServiceMock = null!;
    private Mock<IAuditService> _auditServiceMock = null!;
    private UserService _service = null!;

    public Task InitializeAsync()
    {
        _context = DbFixture.CreateContext<DigitalContext>();
        _documentServiceMock = new Mock<IDocumentService>();
        _auditServiceMock = new Mock<IAuditService>();
        _service = new UserService(
            _documentServiceMock.Object,
            _auditServiceMock.Object,
            _context
        );
        return Task.CompletedTask;
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Return_Error_When_Invalid_Credentials()
    {
        var user = _context.BuildTestUser();

        var result = await _service.UpdatePasswordAsync(user, "wrong_password", "NewPassword123!");
        await Assert.That(result.HasErrorOfType<InvalidCredentialsException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Return_Error_When_Password_Malformed()
    {
        var user = _context.BuildTestUser();

        var result = await _service.UpdatePasswordAsync(user, TestUserFactory.TestUserPassword, "weak");
        await Assert.That(result.HasErrorOfType<PasswordMalformedException>()).IsTrue();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Succeed()
    {
        var user = _context.BuildTestUser();

        var result = await _service.UpdatePasswordAsync(user, TestUserFactory.TestUserPassword, "NewPassword123!");
        await Assert.That(result.HasError).IsFalse();
    }

    [Test]
    public async Task UpdatePasswordAsync_Should_Register_Success_AuditEvent()
    {
        var user = _context.BuildTestUser();

        await _service.UpdatePasswordAsync(user, TestUserFactory.TestUserPassword, "NewPassword123!");
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
        var user = _context.BuildTestUser();

        await _service.UpdatePasswordAsync(user, "wrong_password", "NewPassword123!");
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
        var user = _context.BuildTestUser();

        await _service.UpdatePasswordAsync(user, TestUserFactory.TestUserPassword, "weak");
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
        var user = _context.BuildTestUser();

        var result = await _service.UpdateAvatar(
            user,
            Stream.Null,
            new FileDefinition { FileName = "avatar.jpg", MimeType = "image/jpeg", FileSize = long.MaxValue });
        await Assert.That(result.HasErrorOfType<TooHeavyException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Return_Error_When_Unsupported_Format()
    {
        var user = _context.BuildTestUser();

        var result = await _service.UpdateAvatar(
            user,
            Stream.Null,
            new FileDefinition { FileName = "avatar.json", MimeType = "application/json", FileSize = 100 });
        await Assert.That(result.HasErrorOfType<UnsupportedFormatException>()).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Succeed()
    {
        var user = _context.BuildTestUser();
        var document = new Document
        {
            FileName = $"{Guid.NewGuid()}.jpg",
            MimeType = "image/jpeg",
            FileSize = 100
        };
        await _context.Documents.AddAsync(document);
        await _context.SaveChangesAsync();

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
}
