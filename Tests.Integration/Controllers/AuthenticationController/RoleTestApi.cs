using System.Net;
using Microsoft.Extensions.DependencyInjection;
using SafariDigital.Api;
using SafariDigital.Core.Application;
using SafariDigital.Database.Context;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using Tests.Core.Factories;
using Tests.Core.Integration;
using Tests.Integration.TestUtils.ApiCollections;

namespace Tests.Integration.Controllers.AuthenticationController;

public class RoleTestApi : IntegrationTest
{
    private readonly Repository<User> _userRepository;

    public RoleTestApi(ApiFactory<Program> fixture) : base(fixture)
    {
        _userRepository =
            new Repository<User>(Factory.Services.GetRequiredService<SafariDigitalContext>());
    }

    [Fact]
    public async Task Visitor_ShouldOnlyAccessPublicEndpoints()
    {
        var okResponse = await Client.TestVisitorAuthorization();
        var unauthorizedUserResponse = await Client.TestUserAuthorization();
        var unauthorizedAdminResponse = await Client.TestAdminAuthorization();
        var unauthorizedSuperAdminResponse = await Client.TestSuperAdminAuthorization();
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task Visitor_ShouldNotExceedMaxLoginAttempts()
    {
        var user = Setup();
        var maxAttempts = Configuration.GetSectionOrThrow<int>(EApplicationSetting.SecurityMaxLoginAttempts);
        for (var i = 0; i < maxAttempts; i++)
            await Client.Login(user.Username, "wrongPassword");

        var response = await Client.Login(user.Username, "wrongPassword");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldAccessUserEndpoints()
    {
        var user = Setup();
        await Client.Login(user.Username, user.Password);
        var okResponse = await Client.TestVisitorAuthorization();
        var okUserResponse = await Client.TestUserAuthorization();
        var unauthorizedAdminResponse = await Client.TestAdminAuthorization();
        var unauthorizedSuperAdminResponse = await Client.TestSuperAdminAuthorization();
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_ShouldAccessAdminEndpoints()
    {
        var admin = Setup(UserFactory.CreateUser(EUserRole.Admin));
        await Client.Login(admin.Username, admin.Password);
        var okResponse = await Client.TestVisitorAuthorization();
        var okUserResponse = await Client.TestUserAuthorization();
        var okAdminResponse = await Client.TestAdminAuthorization();
        var unauthorizedSuperAdminResponse = await Client.TestSuperAdminAuthorization();
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_ShouldAccessSuperAdminEndpoints()
    {
        var superAdmin = Setup(UserFactory.CreateUser(EUserRole.SuperAdmin));
        await Client.Login(superAdmin.Username, superAdmin.Password);
        var okResponse = await Client.TestVisitorAuthorization();
        var okUserResponse = await Client.TestUserAuthorization();
        var okAdminResponse = await Client.TestAdminAuthorization();
        var okSuperAdminResponse = await Client.TestSuperAdminAuthorization();
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