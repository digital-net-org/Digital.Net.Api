using System.Net;
using Safari.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Core.Application;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Database;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.AuthenticationController;

public class RoleTestApi : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;


    public RoleTestApi(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        SafariDigitalRepository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task Visitor_ShouldOnlyAccessPublicEndpoints()
    {
        var okResponse = await BaseClient.TestVisitorAuthorization();
        var unauthorizedUserResponse = await BaseClient.TestUserAuthorization();
        var unauthorizedAdminResponse = await BaseClient.TestAdminAuthorization();
        var unauthorizedSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();
        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task Visitor_ShouldNotExceedMaxLoginAttempts()
    {
        var (user, _) = _userFactory.CreateUser();
        var maxAttempts = Configuration.GetSectionOrThrow<int>(EApplicationSetting.SecurityMaxLoginAttempts);
        for (var i = 0; i < maxAttempts; i++) await BaseClient.Login(user.Username, "wrongPassword");
        var response = await BaseClient.Login(user.Username, "wrongPassword");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task User_ShouldAccessUserEndpoints()
    {
        var (user, password) = _userFactory.CreateUser();
        await BaseClient.Login(user.Username, password);

        var okResponse = await BaseClient.TestVisitorAuthorization();
        var okUserResponse = await BaseClient.TestUserAuthorization();
        var unauthorizedAdminResponse = await BaseClient.TestAdminAuthorization();
        var unauthorizedSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();

        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_ShouldAccessAdminEndpoints()
    {
        var (admin, password) = _userFactory.CreateUser(new UserPayload { Role = EUserRole.Admin });
        await BaseClient.Login(admin.Username, password);

        var okResponse = await BaseClient.TestVisitorAuthorization();
        var okUserResponse = await BaseClient.TestUserAuthorization();
        var okAdminResponse = await BaseClient.TestAdminAuthorization();
        var unauthorizedSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();

        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unauthorizedSuperAdminResponse.StatusCode);
    }

    [Fact]
    public async Task SuperAdmin_ShouldAccessSuperAdminEndpoints()
    {
        var (superAdmin, password) = _userFactory.CreateUser(new UserPayload { Role = EUserRole.SuperAdmin });
        await BaseClient.Login(superAdmin.Username, password);

        var okResponse = await BaseClient.TestVisitorAuthorization();
        var okUserResponse = await BaseClient.TestUserAuthorization();
        var okAdminResponse = await BaseClient.TestAdminAuthorization();
        var okSuperAdminResponse = await BaseClient.TestSuperAdminAuthorization();

        Assert.Equal(HttpStatusCode.OK, okResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okUserResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okAdminResponse.StatusCode);
        Assert.Equal(HttpStatusCode.OK, okSuperAdminResponse.StatusCode);
    }
}