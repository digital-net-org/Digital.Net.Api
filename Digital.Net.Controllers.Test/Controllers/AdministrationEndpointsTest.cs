using System.Net;
using System.Net.Http.Json;
using Digital.Net.Controllers.Dto;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Crud.Controllers;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Controllers.Test.Controllers;

public class AdministrationEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private async Task<(User, HttpClient)> CreateTestAdminAsync()
    {
        var admin = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });
        var client = Application.CreateClient();
        await client.Login(admin);
        return (admin, client);
    }

    [Test]
    public async Task AdministrationEndpoints_ShouldBeProtected()
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        await client.Login(user);

        var response = await client.TestAdminAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetUserById_ShouldReturnUser()
    {
        var (user, client) = await CreateTestAdminAsync();
        var createdUser = Application.CreateUser();

        var response = await client.GetUserById(createdUser.Id);
        var userFromResponse = await response.Content.ReadFromJsonAsync<Result<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(userFromResponse!.Value!.Id).IsEqualTo(createdUser.Id);
    }

    [Test]
    public async Task CreateUser_ShouldReturnBadRequest_WhenPasswordIsMissing()
    {
        var (_, client) = await CreateTestAdminAsync();
        var payload = new UserPayload
        {
            Username = "TestCreateUser",
            Login = "testcreate",
            Email = "testcreate@test.com"
        };

        var response = await client.CreateUser(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task GetUsers_ShouldReturnPaginatedUsers()
    {
        var (_, client) = await CreateTestAdminAsync();
        Application.CreateUser();
        Application.CreateUser();
        Application.CreateUser();

        var response = await client.GetUsers(new UserQuery { Size = 2, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Size).IsEqualTo(2);
        await Assert.That(result.Index).IsEqualTo(1);
        await Assert.That(result.Total).IsGreaterThanOrEqualTo(3);
        await Assert.That(result.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetUsers_ShouldFilterByUsername()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser(new TestUserPayload { Username = "FilterTestUser", IsActive = true });
        Application.CreateUser();

        var response = await client.GetUsers(new UserQuery { Username = "FilterTest" });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Total).IsGreaterThanOrEqualTo(1);
        await Assert.That(result.Value.Any(u => u.Id == targetUser.Id)).IsTrue();
    }

    [Test]
    public async Task GetUsers_ShouldFilterByIsActive()
    {
        var (_, client) = await CreateTestAdminAsync();
        Application.CreateUser(new TestUserPayload { IsActive = false });
        Application.CreateUser(new TestUserPayload { IsActive = true });

        var response = await client.GetUsers(new UserQuery { IsActive = false });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(u => u.IsActive == false)).IsTrue();
    }
}