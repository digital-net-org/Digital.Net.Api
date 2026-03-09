using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Core.Messages;
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
}