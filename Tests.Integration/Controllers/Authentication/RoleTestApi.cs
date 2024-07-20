using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Api;
using SafariDigital.Core.Application;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.User;
using SafariDigital.Database.Repository;
using SafariDigital.Services.Authentication.Models;
using Tests.Core.Factories;
using Tests.Core.Integration;
using Tests.Core.Utils;

namespace Tests.Integration.Controllers.Authentication;

public class RoleTestApi : IntegrationTest
{
    private const string LoginApi = "/authentication/login";
    private const string VisitorRoleTestApi = "/authentication/role/visitor/test";
    private const string UserRoleTestApi = "/authentication/role/user/test";
    private const string AdminRoleTestApi = "/authentication/role/admin/test";
    private const string SuperAdminRoleTestApi = "/authentication/role/super-admin/test";
    private readonly Repository<User> _userRepository;

    public RoleTestApi(ApiFactory<Program> fixture) : base(fixture)
    {
        _userRepository =
            new Repository<User>(Factory.Services.GetRequiredService<SafariDigitalContext>());
    }

    [Fact]
    public async Task Visitor_ShouldOnlyAccessPublicEndpoints()
    {
        // Act
        var okResponse = await Client.GetAsync(VisitorRoleTestApi);
        var unauthorizedUserResponse = await Client.GetAsync(UserRoleTestApi);
        var unauthorizedAdminResponse = await Client.GetAsync(AdminRoleTestApi);
        var unauthorizedSuperAdminResponse = await Client.GetAsync(SuperAdminRoleTestApi);

        // Assert
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task Visitor_ShouldNotExceedMaxLoginAttempts()
    {
        // Arrange
        var user = Setup();
        var maxAttempts = Configuration.GetSectionOrThrow<int>(EApplicationSetting.SecurityMaxLoginAttempts);
        for (var i = 0; i < maxAttempts; i++)
            await Client.PostAsJsonAsync(LoginApi, new LoginRequest(user.Username, "wrongPassword"));

        // Act
        var response = await Client.PostAsJsonAsync(LoginApi, new LoginRequest(user.Username, "wrongPassword"));
        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldAccessUserEndpoints()
    {
        // Arrange
        var user = Setup();
        var loginResponse = await Client.PostAsJsonAsync(LoginApi, new LoginRequest(user.Username, user.Password));
        await Client.SetAuthorizations(loginResponse);

        // Act
        var okResponse = await Client.GetAsync(VisitorRoleTestApi);
        var okUserResponse = await Client.GetAsync(UserRoleTestApi);
        var unauthorizedAdminResponse = await Client.GetAsync(AdminRoleTestApi);
        var unauthorizedSuperAdminResponse = await Client.GetAsync(SuperAdminRoleTestApi);

        // Assert
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_ShouldAccessAdminEndpoints()
    {
        // Arrange
        var admin = Setup(UserFactory.CreateUser(EUserRole.Admin));
        var loginResponse = await Client.PostAsJsonAsync(LoginApi, new LoginRequest(admin.Username, admin.Password));
        await Client.SetAuthorizations(loginResponse);

        // Act
        var okResponse = await Client.GetAsync(VisitorRoleTestApi);
        var okUserResponse = await Client.GetAsync(UserRoleTestApi);
        var okAdminResponse = await Client.GetAsync(AdminRoleTestApi);
        var unauthorizedSuperAdminResponse = await Client.GetAsync(SuperAdminRoleTestApi);

        // Assert
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_ShouldAccessSuperAdminEndpoints()
    {
        // Arrange
        var superAdmin = Setup(UserFactory.CreateUser(EUserRole.SuperAdmin));
        var loginResponse = await Client.PostAsJsonAsync(
            LoginApi, new LoginRequest(superAdmin.Username, superAdmin.Password)
        );
        await Client.SetAuthorizations(loginResponse);

        // Act
        var okResponse = await Client.GetAsync(VisitorRoleTestApi);
        var okUserResponse = await Client.GetAsync(UserRoleTestApi);
        var okAdminResponse = await Client.GetAsync(AdminRoleTestApi);
        var okSuperAdminResponse = await Client.GetAsync(SuperAdminRoleTestApi);

        // Assert
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okSuperAdminResponse.StatusCode);
    }

    private User Setup(User? user = null)
    {
        user ??= UserFactory.CreateUser();
        _userRepository.Create(UserFactory.CreateUserWithHashedPassword(user));
        _userRepository.Save();
        var retrieved = _userRepository.Get(u => u.Username == user.Username).FirstOrDefault()!;
        retrieved.Password = user.Password;
        return retrieved;
    }
}