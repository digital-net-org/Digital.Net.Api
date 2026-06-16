using System.Net;
using System.Net.Http.Json;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Tests.Core.Http.Endpoints;

public class AdministrationEndpointsTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    private async Task<(User, HttpClient)> CreateTestAdminAsync()
    {
        var admin = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });
        var client = ApplicationFixture.CreateClient();
        await client.Login(admin);
        return (admin, client);
    }

    [Test]
    public async Task AdministrationEndpoints_ShouldBeProtected()
    {
        var client = ApplicationFixture.CreateClient();
        var user = ApplicationFixture.CreateUser();
        await client.Login(user);

        var response = await client.TestAdminAuthorization();
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task GetUserById_ShouldReturnUser()
    {
        var (user, client) = await CreateTestAdminAsync();
        var createdUser = ApplicationFixture.CreateUser();

        var response = await client.GetUserById(createdUser.Id);
        var userFromResponse = await response.Content.ReadFromJsonAsync<Result<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(userFromResponse!.Value!.Id).IsEqualTo(createdUser.Id);
    }

    [Test]
    public async Task CreateUser_ShouldCreateUser()
    {
        var (_, client) = await CreateTestAdminAsync();
        var payload = new UserPayload
        {
            Username = "NewTestUser",
            Login = "newtestuser",
            Email = "newuser@test.com",
            Password = "ValidPassword123!"
        };

        var response = await client.CreateUser(payload);
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotEqualTo(Guid.Empty);

        var createdUser = await ApplicationFixture.GetContext().Users.FindAsync(result.Value);
        await Assert.That(createdUser).IsNotNull();
        await Assert.That(createdUser!.Username).IsEqualTo("NewTestUser");
        await Assert.That(createdUser.Email).IsEqualTo("newuser@test.com");
        await Assert.That(createdUser.Login).IsEqualTo("newtestuser");
        await Assert.That(createdUser.IsActive).IsFalse();
        await Assert.That(createdUser.IsAdmin).IsFalse();
    }

    [Test]
    public async Task CreateUser_ShouldReturnBadRequest_WhenPasswordMalformed()
    {
        var (_, client) = await CreateTestAdminAsync();
        var payload = new UserPayload
        {
            Username = "TestMalformed",
            Login = "testmalformed",
            Email = "malformed@test.com",
            Password = "weak"
        };

        var response = await client.CreateUser(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task DeleteUser_ShouldDeleteUser()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = ApplicationFixture.CreateUser();
        var deletePayload = new UserDeletePayload { Password = TestUserFactory.TestUserPassword };

        var response = await client.DeleteUser(targetUser.Id, deletePayload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var deletedUser = await ApplicationFixture.GetContext().Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUser.Id);
        await Assert.That(deletedUser).IsNull();
    }

    [Test]
    public async Task DeleteUser_ShouldReturnUnauthorized_WhenPasswordInvalid()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = ApplicationFixture.CreateUser();
        var deletePayload = new UserDeletePayload { Password = "WrongPassword123!" };

        var response = await client.DeleteUser(targetUser.Id, deletePayload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetUsers_ShouldReturnPaginatedUsers()
    {
        var (_, client) = await CreateTestAdminAsync();
        ApplicationFixture.CreateUser();
        ApplicationFixture.CreateUser();
        ApplicationFixture.CreateUser();

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
        var targetUser = ApplicationFixture.CreateUser(new TestUserPayload
            { Username = "FilterTestUser", IsActive = true });
        ApplicationFixture.CreateUser();

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
        ApplicationFixture.CreateUser(new TestUserPayload { IsActive = false });
        ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });

        var response = await client.GetUsers(new UserQuery { IsActive = false });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<UserDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(u => u.IsActive == false)).IsTrue();
    }

    [Test]
    public async Task UpdateUserStatus_ShouldActivateUser()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = false });

        var response = await client.UpdateUserStatus(targetUser.Id, new UserStatusPayload { IsActive = true });

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var updatedUser = await ApplicationFixture.GetContext().Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == targetUser.Id);
        await Assert.That(updatedUser.IsActive).IsTrue();
    }

    [Test]
    public async Task UpdateUserStatus_ShouldDeactivateUser()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });

        var response = await client.UpdateUserStatus(targetUser.Id, new UserStatusPayload { IsActive = false });

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var updatedUser = await ApplicationFixture.GetContext().Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == targetUser.Id);
        await Assert.That(updatedUser.IsActive).IsFalse();
    }

    [Test]
    public async Task UpdateUserRole_ShouldPromoteToAdmin()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });
        var payload = new UserRolePayload { IsAdmin = true, Password = TestUserFactory.TestUserPassword };

        var response = await client.UpdateUserRole(targetUser.Id, payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var updatedUser = await ApplicationFixture.GetContext().Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == targetUser.Id);
        await Assert.That(updatedUser.IsAdmin).IsTrue();
    }

    [Test]
    public async Task UpdateUserRole_ShouldReturnUnauthorized_WhenPasswordInvalid()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = ApplicationFixture.CreateUser();
        var payload = new UserRolePayload { IsAdmin = true, Password = "WrongPassword123!" };

        var response = await client.UpdateUserRole(targetUser.Id, payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}
