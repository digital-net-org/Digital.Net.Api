using System.Net;
using System.Net.Http.Json;
using Digital.Net.Api.Controllers.Dto;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Controllers.Test.Controllers;

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
        await Assert.That(result!.Value!.Id).IsEqualTo(user.Id);
        await Assert.That(result.Value!.Username).IsEqualTo(user.Username);
        await Assert.That(result.Value!.Email).IsEqualTo(user.Email);
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

        await Assert.That(self!.Value!.Username).IsEqualTo("UpdatedName");
        await Assert.That(self.Value!.Email).IsEqualTo("updated@test.com");
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
}