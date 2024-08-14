using Microsoft.AspNetCore.JsonPatch;
using Safari.Net.Data.Repositories;
using Safari.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Database;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.UserController;

public class PatchUserApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;
    private readonly IRepository<User> _userRepository;

    public PatchUserApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        _userRepository = new Repository<User, SafariDigitalContext>(GetContext());
        _userFactory = new UserFactory(_userRepository);
    }

    [Fact]
    public async Task PatchUser_ReturnsSuccess_WhenValidPayload()
    {
        var user = Setup();
        var patch = new JsonPatchDocument<User>();
        await BaseClient.PatchUser(user.Id, "[{\"op\":\"replace\",\"path\":\"/username\",\"value\":\"new_username\"}]");
        var updatedUser = await _userRepository.GetByIdAsync(user.Id);
        Assert.Equal("new_username", updatedUser?.Username);
        _userFactory.Dispose();
    }

    [Fact]
    public async Task PatchUser_ReturnsUnauthorized_WhenUserNotAuthorized() => throw new NotImplementedException();

    [Fact]
    public async Task PatchUser_ReturnsError_WhenInvalidPayload() => throw new NotImplementedException();

    private User Setup()
    {
        var (user, password) = _userFactory.CreateUser();
        BaseClient.Login(user.Username, password).Wait();
        return user;
    }
}