using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Users.Events;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Test.Endpoints;

public class UserEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private async Task<(User, HttpClient)> CreateAuthenticatedUserAsync(TestUserPayload? payload = null)
    {
        var user = Application.CreateUser(payload ?? new TestUserPayload { IsActive = true });
        var client = Application.CreateClient();
        await client.Login(user);
        return (user, client);
    }

    [Test]
    public async Task GetSelf_ShouldReturnAuthenticatedUser()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();

        var response = await client.GetSelf();
        var result = await response.Content.ReadFromJsonAsync<Result<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).EqualTo(user.Id);
        await Assert.That(result.Value!.Username).EqualTo(user.Username);
        await Assert.That(result.Value!.Email).EqualTo(user.Email);
        await Assert.That(result.Value!.IsActive).EqualTo(user.IsActive);
    }

    [Test]
    public async Task PatchSelf_ShouldUpdateWritableFields()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[]
        {
            new { op = "replace", path = "/Username", value = "UpdatedName" },
            new { op = "replace", path = "/Email", value = "updated@test.com" }
        };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var selfResponse = await client.GetSelf();
        var self = await selfResponse.Content.ReadFromJsonAsync<Result<UserDto>>();

        await Assert.That(self!.Value!.Username).EqualTo("UpdatedName");
        await Assert.That(self.Value!.Email).EqualTo("updated@test.com");
    }

    [Test]
    public async Task PatchSelf_ShouldRejectReadOnlyField_IsAdmin()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/IsAdmin", value = "true" } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchSelf_ShouldRejectReadOnlyField_IsActive()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/IsActive", value = "false" } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchSelf_ShouldRejectReadOnlyField_Login()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/Login", value = "hackedlogin" } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchSelf_ShouldRejectReadOnlyField_Password()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/Password", value = "HackedPassword1!" } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchSelf_ShouldCreateAuditEvent_WhenUsernameUpdated()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/Username", value = "AuditUser" } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == user.Id && e.Name == UserEvents.UpdateProfile)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
        await Assert.That(auditEvent.Payload).Contains("Username");
    }

    [Test]
    public async Task PatchSelf_ShouldCreateAuditEvent_WhenEmailUpdated()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/Email", value = "auditemail@test.com" } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == user.Id && e.Name == UserEvents.UpdateProfile)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
        await Assert.That(auditEvent.Payload).Contains("Email");
    }

    [Test]
    public async Task PatchSelf_ShouldCreateFailedAuditEvent_WhenReadOnlyFieldPatched()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/IsAdmin", value = "true" } };
        await client.PatchSelf(patch);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == user.Id && e.Name == UserEvents.UpdateProfile)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Failed);
        await Assert.That(auditEvent.HasError).IsTrue();
    }

    [Test]
    public async Task PatchSelf_ShouldRejectReadOnlyAvatarMutation()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/Avatar", value = new { Id = Guid.NewGuid() } } };
        var response = await client.PatchSelf(patch);
        var result = await response.Content.ReadFromJsonAsync<Result>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
        await Assert.That(result!.Errors.Any(e => e.Reference == "DIGITAL_NET_ENTITIES_EXCEPTIONS_ENTITYVALIDATIONEXCEPTION")).IsTrue();
    }

    [Test]
    public async Task PatchSelf_ShouldRejectReadOnlyApiKeyMutation()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "add", path = "/ApiKeys/-", value = new { Name = "hacked", Key = "xxx" } } };
        var response = await client.PatchSelf(patch);
        var result = await response.Content.ReadFromJsonAsync<Result>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
        await Assert.That(result!.Errors.Any(e => e.Reference == "DIGITAL_NET_ENTITIES_EXCEPTIONS_ENTITYVALIDATIONEXCEPTION")).IsTrue();
    }

    [Test]
    public async Task PatchSelf_ShouldRejectInvalidEmail()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var patch = new[] { new { op = "replace", path = "/Email", value = "not-an-email" } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchSelf_ShouldRejectDuplicateEmail()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();
        var otherUser = Application.CreateUser(new TestUserPayload
        {
            IsActive = true,
            Email = "taken@test.com"
        });

        var patch = new[] { new { op = "replace", path = "/Email", value = otherUser.Email } };
        var response = await client.PatchSelf(patch);

        await Assert.That(response.StatusCode).IsNotEqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task UpdatePassword_ShouldUpdatePassword()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        const string newPassword = "NewPassword123!";

        var response = await client.UpdatePassword(TestUserFactory.TestUserPassword, newPassword);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var loginClient = Application.CreateClient();
        var loginResponse = await loginClient.Login(user.Login, newPassword);

        await Assert.That(loginResponse.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task UpdatePassword_ShouldCreateAuditEvent_WhenSuccessful()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        const string newPassword = "NewPassword123!";

        var response = await client.UpdatePassword(TestUserFactory.TestUserPassword, newPassword);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == user.Id && e.Name == UserEvents.UpdatePassword)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
        await Assert.That(auditEvent.HasError).IsFalse();
    }

    [Test]
    public async Task UpdatePassword_ShouldCreateAuditEvent_WhenFailed()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();

        var response = await client.UpdatePassword("WrongPassword!", "NewPassword123!");
        await Assert.That(response.StatusCode).IsNotEqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == user.Id && e.Name == UserEvents.UpdatePassword)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Failed);
        await Assert.That(auditEvent.HasError).IsTrue();
    }

    [Test]
    public async Task UpdatePassword_ShouldRejectWeakPassword()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var response = await client.UpdatePassword(TestUserFactory.TestUserPassword, "weak");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    private static MultipartFormDataContent CreateValidAvatarPayload()
    {
        var imageBytes =
            Convert.FromBase64String(
                "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mP8z8BQDwAEhQGAhKmMIQAAAABJRU5ErkJggg=="); // 1x1 transparent png
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(imageBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("image/png");
        content.Add(fileContent, "avatar", "avatar.png");
        return content;
    }

    private static MultipartFormDataContent CreateInvalidAvatarPayload()
    {
        var textBytes = "This is not an image."u8.ToArray();
        var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(textBytes);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("text/plain");
        content.Add(fileContent, "avatar", "avatar.txt");
        return content;
    }

    [Test]
    public async Task UpdateAvatar_Should_Succeed()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        using var content = CreateValidAvatarPayload();

        var uploadResponse = await client.UpdateAvatar(content);
        await Assert.That((int)uploadResponse.StatusCode).EqualTo((int)HttpStatusCode.OK);

        var db = Application.GetContext();
        var dbUser = await db.Users
            .AsNoTracking()
            .Include(u => u.Avatar)
            .ThenInclude(a => a!.Document)
            .FirstAsync(u => u.Id == user.Id);

        await Assert.That(dbUser.AvatarId).IsNotNull();
        await Assert.That(dbUser.Avatar).IsNotNull();
        await Assert.That(dbUser.Avatar!.Document).IsNotNull();

        var documentService = Application.GetService<IDocumentService>();
        var expectedFilePath = documentService.GetDocumentPath(dbUser.Avatar.Document!);
        await Assert.That(File.Exists(expectedFilePath)).IsTrue();
    }

    [Test]
    public async Task UpdateAvatar_Should_Reject_InvalidFormat()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();
        using var content = CreateInvalidAvatarPayload();

        var uploadResponse = await client.UpdateAvatar(content);
        await Assert.That((int)uploadResponse.StatusCode).IsEqualTo((int)HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task RemoveAvatar_Should_Succeed()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        using var content = CreateValidAvatarPayload();
        await client.UpdateAvatar(content);

        var db = Application.GetContext();
        var documentService = Application.GetService<IDocumentService>();

        var dbUserBefore = await db.Users
            .AsNoTracking()
            .Include(u => u.Avatar)
            .ThenInclude(a => a!.Document)
            .FirstAsync(u => u.Id == user.Id);

        var expectedFilePath = documentService.GetDocumentPath(dbUserBefore.Avatar!.Document!);
        var removeResponse = await client.RemoveAvatar();
        await Assert.That((int)removeResponse.StatusCode).IsEqualTo((int)HttpStatusCode.OK);

        var dbUserAfter = await db.Users
            .AsNoTracking()
            .Include(u => u.Avatar)
            .FirstAsync(u => u.Id == user.Id);

        await Assert.That(dbUserAfter.AvatarId).IsNull();
        await Assert.That(dbUserAfter.Avatar).IsNull();
        await Assert.That(File.Exists(expectedFilePath)).IsFalse();
    }

    [Test]
    public async Task RemoveAvatar_Should_Succeed_WhenNoAvatarExists()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var removeResponse = await client.RemoveAvatar();
        await Assert.That((int)removeResponse.StatusCode).IsEqualTo((int)HttpStatusCode.OK);
    }

    [Test]
    public async Task GetUserAvatar_WithoutToken_ShouldReturnUnauthorized()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        using var content = CreateValidAvatarPayload();
        await client.UpdateAvatar(content);

        var visitorClient = Application.CreateClient();
        var response = await visitorClient.GetUserAvatar(user.Id);
        await Assert.That((int)response.StatusCode).IsEqualTo((int)HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetUserAvatar_Should_ReturnFile_WithETagHeader()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        using var content = CreateValidAvatarPayload();
        await client.UpdateAvatar(content);

        var db = Application.GetContext();
        var dbUser = await db.Users
            .AsNoTracking()
            .Include(u => u.Avatar)
            .ThenInclude(a => a!.Document)
            .FirstAsync(u => u.Id == user.Id);
        var expectedEtag = dbUser.Avatar!.Document!.Id.ToString();

        var response = await client.GetUserAvatar(user.Id);
        await Assert.That((int)response.StatusCode).IsEqualTo((int)HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType?.MediaType).IsNotNull();

        var bytes = await response.Content.ReadAsByteArrayAsync();
        await Assert.That(bytes.Length).IsGreaterThan(0);

        var etag = response.Headers.ETag?.Tag;
        await Assert.That(etag).IsNotNull();
        await Assert.That(etag!.Trim('"')).IsEqualTo(expectedEtag);
    }

    [Test]
    public async Task GetUserAvatar_Should_Return304_WhenETagMatches()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        using var content = CreateValidAvatarPayload();
        await client.UpdateAvatar(content);

        var firstResponse = await client.GetUserAvatar(user.Id);
        var etag = firstResponse.Headers.ETag?.Tag;
        await Assert.That(etag).IsNotNull();

        var cachedResponse = await client.GetUserAvatar(user.Id, etag!);
        await Assert.That((int)cachedResponse.StatusCode).IsEqualTo(304);

        var cachedBytes = await cachedResponse.Content.ReadAsByteArrayAsync();
        await Assert.That(cachedBytes.Length).IsEqualTo(0);
    }

    [Test]
    public async Task GetUserAvatar_Should_ReturnFile_WhenETagDoesNotMatch()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();
        using var content = CreateValidAvatarPayload();
        await client.UpdateAvatar(content);

        var response = await client.GetUserAvatar(user.Id, "\"wrong-etag\"");
        await Assert.That((int)response.StatusCode).IsEqualTo((int)HttpStatusCode.OK);

        var bytes = await response.Content.ReadAsByteArrayAsync();
        await Assert.That(bytes.Length).IsGreaterThan(0);
    }

    [Test]
    public async Task GetUserAvatar_Should_ReturnNotFound_WhenNoAvatar()
    {
        var (user, client) = await CreateAuthenticatedUserAsync();

        var response = await client.GetUserAvatar(user.Id);
        await Assert.That((int)response.StatusCode).IsEqualTo((int)HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetUserAvatar_Should_ReturnNotFound_WhenUserDoesNotExist()
    {
        var (_, client) = await CreateAuthenticatedUserAsync();

        var response = await client.GetUserAvatar(Guid.NewGuid());
        await Assert.That((int)response.StatusCode).IsEqualTo((int)HttpStatusCode.NotFound);
    }
}