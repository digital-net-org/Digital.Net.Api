using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Digital.Net.Api.Endpoints.Dto;
using Digital.Net.Api.Services.Users.Events;
using Digital.Net.Api.Services.Users.Events.Types;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Crud.Enpoints;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Test.Endpoints;

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

        var createdUser = await Application.GetContext().Users.FindAsync(result.Value);
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
    public async Task CreateUser_ShouldGenerateAuditEvent()
    {
        var (admin, client) = await CreateTestAdminAsync();
        var payload = new UserPayload
        {
            Username = "AuditTestUser",
            Login = "audittestuser",
            Email = "audit@test.com",
            Password = "ValidPassword123!"
        };

        var response = await client.CreateUser(payload);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == admin.Id && e.Name == UserEvents.CreateUser)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
        await Assert.That(auditEvent.Payload).IsNotNull();

        var eventPayload = JsonSerializer.Deserialize<AdminUserMutationEvent>(auditEvent.Payload!);
        await Assert.That(eventPayload).IsNotNull();
        await Assert.That(eventPayload!.Username).IsEqualTo("AuditTestUser");
        await Assert.That(eventPayload.Email).IsEqualTo("audit@test.com");
        await Assert.That(eventPayload.Login).IsEqualTo("audittestuser");
    }

    [Test]
    public async Task DeleteUser_ShouldDeleteUser()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser();
        var deletePayload = new UserDeletePayload { Password = TestUserFactory.TestUserPassword };

        var response = await client.DeleteUser(targetUser.Id, deletePayload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var deletedUser = await Application.GetContext().Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == targetUser.Id);
        await Assert.That(deletedUser).IsNull();
    }

    [Test]
    public async Task DeleteUser_ShouldReturnUnauthorized_WhenPasswordInvalid()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser();
        var deletePayload = new UserDeletePayload { Password = "WrongPassword123!" };

        var response = await client.DeleteUser(targetUser.Id, deletePayload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task DeleteUser_ShouldReturnForbidden_WhenTargetIsAdmin()
    {
        var (_, client) = await CreateTestAdminAsync();
        var adminTarget = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });
        var deletePayload = new UserDeletePayload { Password = TestUserFactory.TestUserPassword };

        var response = await client.DeleteUser(adminTarget.Id, deletePayload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task DeleteUser_ShouldGenerateAuditEvent()
    {
        var (admin, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser();
        var deletePayload = new UserDeletePayload { Password = TestUserFactory.TestUserPassword };

        var response = await client.DeleteUser(targetUser.Id, deletePayload);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == admin.Id && e.Name == UserEvents.DeleteUser)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
        await Assert.That(auditEvent.Payload).IsNotNull();

        var eventPayload = JsonSerializer.Deserialize<AdminUserMutationEvent>(auditEvent.Payload!);
        await Assert.That(eventPayload).IsNotNull();
        await Assert.That(eventPayload!.Id).IsEqualTo(targetUser.Id);
    }

    [Test]
    public async Task DeleteUser_ShouldGenerateSecurityEvent_WhenTargetIsAdmin()
    {
        var (admin, client) = await CreateTestAdminAsync();
        var adminTarget = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });
        var deletePayload = new UserDeletePayload { Password = TestUserFactory.TestUserPassword };

        var response = await client.DeleteUser(adminTarget.Id, deletePayload);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);

        var securityEvent = await Application.GetContext().Events
            .Where(e => e.UserId == admin.Id && e.Name == UserEvents.DeleteUser && e.HasError)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(securityEvent.State).EqualTo(EventState.Failed);
        await Assert.That(securityEvent.Payload).IsNotNull();

        var eventPayload = JsonSerializer.Deserialize<AdminUserMutationEvent>(securityEvent.Payload!);
        await Assert.That(eventPayload).IsNotNull();
        await Assert.That(eventPayload!.Id).IsEqualTo(adminTarget.Id);
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

    [Test]
    public async Task UpdateUserStatus_ShouldActivateUser()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser(new TestUserPayload { IsActive = false });

        var response = await client.UpdateUserStatus(targetUser.Id, new UserStatusPayload { IsActive = true });

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var updatedUser = await Application.GetContext().Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == targetUser.Id);
        await Assert.That(updatedUser.IsActive).IsTrue();
    }

    [Test]
    public async Task UpdateUserStatus_ShouldDeactivateUser()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser(new TestUserPayload { IsActive = true });

        var response = await client.UpdateUserStatus(targetUser.Id, new UserStatusPayload { IsActive = false });

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var updatedUser = await Application.GetContext().Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == targetUser.Id);
        await Assert.That(updatedUser.IsActive).IsFalse();
    }

    [Test]
    public async Task UpdateUserStatus_ShouldReturnForbidden_WhenDeactivatingAdmin()
    {
        var (_, client) = await CreateTestAdminAsync();
        var adminTarget = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });

        var response = await client.UpdateUserStatus(adminTarget.Id, new UserStatusPayload { IsActive = false });

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task UpdateUserStatus_ShouldGenerateAuditEvent()
    {
        var (admin, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser(new TestUserPayload { IsActive = false });

        var response = await client.UpdateUserStatus(targetUser.Id, new UserStatusPayload { IsActive = true });
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == admin.Id && e.Name == UserEvents.UpdateUserStatus && !e.HasError)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
        await Assert.That(auditEvent.Payload).IsNotNull();

        var eventPayload = JsonSerializer.Deserialize<AdminUserMutationEvent>(auditEvent.Payload!);
        await Assert.That(eventPayload).IsNotNull();
        await Assert.That(eventPayload!.Id).IsEqualTo(targetUser.Id);
    }

    [Test]
    public async Task UpdateUserStatus_ShouldGenerateSecurityEvent_WhenDeactivatingAdmin()
    {
        var (admin, client) = await CreateTestAdminAsync();
        var adminTarget = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });

        var response = await client.UpdateUserStatus(adminTarget.Id, new UserStatusPayload { IsActive = false });
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);

        var securityEvent = await Application.GetContext().Events
            .Where(e => e.UserId == admin.Id && e.Name == UserEvents.UpdateUserStatus && e.HasError)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(securityEvent.State).EqualTo(EventState.Failed);
        await Assert.That(securityEvent.Payload).IsNotNull();

        var eventPayload = JsonSerializer.Deserialize<AdminUserMutationEvent>(securityEvent.Payload!);
        await Assert.That(eventPayload).IsNotNull();
        await Assert.That(eventPayload!.Id).IsEqualTo(adminTarget.Id);
    }

    [Test]
    public async Task UpdateUserRole_ShouldPromoteToAdmin()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser(new TestUserPayload { IsActive = true });
        var payload = new UserRolePayload { IsAdmin = true, Password = TestUserFactory.TestUserPassword };

        var response = await client.UpdateUserRole(targetUser.Id, payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var updatedUser = await Application.GetContext().Users
            .AsNoTracking()
            .FirstAsync(u => u.Id == targetUser.Id);
        await Assert.That(updatedUser.IsAdmin).IsTrue();
    }

    [Test]
    public async Task UpdateUserRole_ShouldReturnForbidden_WhenDemotingAdmin()
    {
        var (_, client) = await CreateTestAdminAsync();
        var adminTarget = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });
        var payload = new UserRolePayload { IsAdmin = false, Password = TestUserFactory.TestUserPassword };

        var response = await client.UpdateUserRole(adminTarget.Id, payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);
    }

    [Test]
    public async Task UpdateUserRole_ShouldReturnUnauthorized_WhenPasswordInvalid()
    {
        var (_, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser();
        var payload = new UserRolePayload { IsAdmin = true, Password = "WrongPassword123!" };

        var response = await client.UpdateUserRole(targetUser.Id, payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task UpdateUserRole_ShouldGenerateAuditEvent()
    {
        var (admin, client) = await CreateTestAdminAsync();
        var targetUser = Application.CreateUser(new TestUserPayload { IsActive = true });
        var payload = new UserRolePayload { IsAdmin = true, Password = TestUserFactory.TestUserPassword };

        var response = await client.UpdateUserRole(targetUser.Id, payload);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var auditEvent = await Application.GetContext().Events
            .Where(e => e.UserId == admin.Id && e.Name == UserEvents.UpdateUserRole && !e.HasError)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(auditEvent.State).EqualTo(EventState.Success);
        await Assert.That(auditEvent.Payload).IsNotNull();

        var eventPayload = JsonSerializer.Deserialize<AdminUserMutationEvent>(auditEvent.Payload!);
        await Assert.That(eventPayload).IsNotNull();
        await Assert.That(eventPayload!.Id).IsEqualTo(targetUser.Id);
    }

    [Test]
    public async Task UpdateUserRole_ShouldGenerateSecurityEvent_WhenDemotingAdmin()
    {
        var (admin, client) = await CreateTestAdminAsync();
        var adminTarget = Application.CreateUser(new TestUserPayload { IsActive = true, IsAdmin = true });
        var payload = new UserRolePayload { IsAdmin = false, Password = TestUserFactory.TestUserPassword };

        var response = await client.UpdateUserRole(adminTarget.Id, payload);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Forbidden);

        var securityEvent = await Application.GetContext().Events
            .Where(e => e.UserId == admin.Id && e.Name == UserEvents.UpdateUserRole && e.HasError)
            .OrderByDescending(e => e.CreatedAt)
            .FirstAsync();

        await Assert.That(securityEvent.State).EqualTo(EventState.Failed);
        await Assert.That(securityEvent.Payload).IsNotNull();

        var eventPayload = JsonSerializer.Deserialize<AdminUserMutationEvent>(securityEvent.Payload!);
        await Assert.That(eventPayload).IsNotNull();
        await Assert.That(eventPayload!.Id).IsEqualTo(adminTarget.Id);
    }
}