using System.Net;
using System.Net.Http.Json;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.Services.ApiKeys;
using Digital.Net.Api.Services.Users.Events;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.ApiKeys;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Test.Endpoints;

public class ApiKeyEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private async Task<HttpClient> CreateAuthenticatedClientAsync(TestUserPayload? payload = null)
    {
        var user = Application.CreateUser(payload ?? new TestUserPayload { IsActive = true });
        var client = Application.CreateClient();
        await client.Login(user);
        return client;
    }

    [Test]
    public async Task Create_ShouldReturnPlaintextKey()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateApiKey("My Test Key");
        var result = await response.Content.ReadFromJsonAsync<Result<string>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotNull();
        await Assert.That(result.Value!.Length).IsGreaterThan(0);

        // Verify the key is stored as hash: query DB for matching hash
        var db = Application.GetContext();
        var hashedKey = ApiKey.Hash(result.Value);
        var exists = await db.ApiKeys.AnyAsync(k => k.Key == hashedKey);
        await Assert.That(exists).IsTrue();
    }

    [Test]
    public async Task Create_ShouldDefaultExpirationTo3Months()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateApiKey("Default Expiry Key");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var db = Application.GetContext();
        var storedKey = await db.ApiKeys.OrderByDescending(k => k.CreatedAt).FirstAsync(k => k.Name == "Default Expiry Key");
        var expectedExpiry = DateTime.UtcNow.Add(ApiKeyService.DefaultExpiration);

        await Assert.That(storedKey.ExpiredAt).IsNotNull();
        await Assert.That((storedKey.ExpiredAt!.Value - expectedExpiry).TotalMinutes).IsLessThan(1);
    }

    [Test]
    public async Task Create_ShouldRespectCustomExpiration()
    {
        var client = await CreateAuthenticatedClientAsync();
        var customExpiry = DateTime.UtcNow.AddYears(1);

        var response = await client.CreateApiKey("Custom Expiry Key", customExpiry);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var db = Application.GetContext();
        var storedKey = await db.ApiKeys.OrderByDescending(k => k.CreatedAt).FirstAsync(k => k.Name == "Custom Expiry Key");
        await Assert.That((storedKey.ExpiredAt!.Value - customExpiry).TotalMinutes).IsLessThan(1);
    }

    [Test]
    public async Task Create_ShouldRejectPastExpirationDate()
    {
        var client = await CreateAuthenticatedClientAsync();
        var pastDate = DateTime.UtcNow.AddDays(-1);

        var response = await client.CreateApiKey("Past Key", pastDate);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Create_ShouldRejectWhenMaxKeysReached()
    {
        var client = await CreateAuthenticatedClientAsync();

        for (var i = 0; i < ApiKeyService.MaxApiKeysPerUser; i++)
            await client.CreateApiKey($"Key {i}");

        var response = await client.CreateApiKey("One Too Many");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Create_ShouldRejectDuplicateName()
    {
        var client = await CreateAuthenticatedClientAsync();

        await client.CreateApiKey("Duplicate Name");
        var response = await client.CreateApiKey("Duplicate Name");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Create_ShouldAllowSameNameForDifferentUsers()
    {
        var client1 = await CreateAuthenticatedClientAsync();
        var client2 = await CreateAuthenticatedClientAsync();

        var response1 = await client1.CreateApiKey("Shared Name");
        var response2 = await client2.CreateApiKey("Shared Name");

        await Assert.That(response1.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response2.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task Create_ShouldRejectInvalidName()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateApiKey("Invalid!@#$%");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Create_ShouldRejectEmptyName()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.CreateApiKey("");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task Create_ShouldGenerateAuditEvent()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        var client = Application.CreateClient();
        await client.Login(user);

        var response = await client.CreateApiKey("Audited Key");
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == user.Id && e.Name == UserEvents.CreateApiKey)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
    }

    [Test]
    public async Task List_ShouldReturnMetadataOnly()
    {
        var client = await CreateAuthenticatedClientAsync();
        await client.CreateApiKey("Listed Key");

        var response = await client.ListApiKeys();
        var result = await response.Content.ReadFromJsonAsync<Result<List<ApiKeyDto>>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Count).IsGreaterThan(0);

        var key = result.Value!.First(k => k.Name == "Listed Key");
        await Assert.That(key.Name).EqualTo("Listed Key");
        await Assert.That(key.Id).IsNotEqualTo(Guid.Empty);
        await Assert.That(key.CreatedAt).IsNotEqualTo(default(DateTime));
    }

    [Test]
    public async Task List_ShouldReturnOnlyOwnKeys()
    {
        var client1 = await CreateAuthenticatedClientAsync();
        var client2 = await CreateAuthenticatedClientAsync();

        await client1.CreateApiKey("User1 Key");
        await client2.CreateApiKey("User2 Key");

        var response = await client1.ListApiKeys();
        var result = await response.Content.ReadFromJsonAsync<Result<List<ApiKeyDto>>>();

        await Assert.That(result!.Value!.Any(k => k.Name == "User1 Key")).IsTrue();
        await Assert.That(result.Value!.Any(k => k.Name == "User2 Key")).IsFalse();
    }

    [Test]
    public async Task Delete_ShouldRevokeKey()
    {
        var client = await CreateAuthenticatedClientAsync();
        var createResponse = await client.CreateApiKey("To Delete");
        await Assert.That(createResponse.StatusCode).EqualTo(HttpStatusCode.OK);

        var listResponse = await client.ListApiKeys();
        var listResult = await listResponse.Content.ReadFromJsonAsync<Result<List<ApiKeyDto>>>();
        var keyId = listResult!.Value!.First(k => k.Name == "To Delete").Id;

        var deleteResponse = await client.DeleteApiKey(keyId);
        await Assert.That(deleteResponse.StatusCode).EqualTo(HttpStatusCode.OK);

        var afterDeleteResponse = await client.ListApiKeys();
        var afterDeleteResult = await afterDeleteResponse.Content.ReadFromJsonAsync<Result<List<ApiKeyDto>>>();
        await Assert.That(afterDeleteResult!.Value!.Any(k => k.Name == "To Delete")).IsFalse();
    }

    [Test]
    public async Task Delete_ShouldRejectOtherUsersKey()
    {
        var client1 = await CreateAuthenticatedClientAsync();
        var client2 = await CreateAuthenticatedClientAsync();

        await client1.CreateApiKey("Owner Key");

        var listResponse = await client1.ListApiKeys();
        var listResult = await listResponse.Content.ReadFromJsonAsync<Result<List<ApiKeyDto>>>();
        var keyId = listResult!.Value!.First(k => k.Name == "Owner Key").Id;

        var deleteResponse = await client2.DeleteApiKey(keyId);
        await Assert.That(deleteResponse.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Delete_ShouldReturnNotFound_WhenKeyDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.DeleteApiKey(Guid.NewGuid());
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task Delete_ShouldGenerateAuditEvent()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        var client = Application.CreateClient();
        await client.Login(user);

        await client.CreateApiKey("Delete Audit Key");

        var listResponse = await client.ListApiKeys();
        var listResult = await listResponse.Content.ReadFromJsonAsync<Result<List<ApiKeyDto>>>();
        var keyId = listResult!.Value!.First(k => k.Name == "Delete Audit Key").Id;

        var deleteResponse = await client.DeleteApiKey(keyId);
        await Assert.That(deleteResponse.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == user.Id && e.Name == UserEvents.DeleteApiKey)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
    }
}
