using Safari.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Database;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.UserController;

public class PatchUserApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;

    public PatchUserApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        SafariDigitalRepository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
    }

    [Fact]
    public async Task PatchUser_ReturnsSuccess_WhenValidPayload() => throw new NotImplementedException();

    [Fact]
    public async Task PatchUser_ReturnsUnauthorized_WhenUserNotAuthorized() => throw new NotImplementedException();

    [Fact]
    public async Task PatchUser_ReturnsError_WhenInvalidPayload() => throw new NotImplementedException();
}