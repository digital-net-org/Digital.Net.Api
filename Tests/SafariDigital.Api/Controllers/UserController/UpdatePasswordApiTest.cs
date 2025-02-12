using System.Net;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Users;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.UserController;

public class UpdatePasswordApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;

    public UpdatePasswordApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        Repository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task UpdatePassword_ShouldReturnOk()
    {
        var (user, password) = _userFactory.CreateUser();
        var res = await BaseClient.Login(user.Login, password);
        var response = await BaseClient.UpdatePassword(user.Id, password, "1newPassword*");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}