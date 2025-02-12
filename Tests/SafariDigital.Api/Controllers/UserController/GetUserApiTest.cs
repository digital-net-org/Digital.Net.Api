using System.Net.Http.Json;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Core.Random;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.Mvc.Controllers.Pagination;
using Digital.Lib.Net.TestTools.Integration;
using SafariDigital.Api;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Context;
using SafariDigital.Data.Models.Users;
using Tests.Utils.ApiCollections;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Api.Controllers.UserController;

public class GetUserApiTest : IntegrationTest<Program, SafariDigitalContext>
{
    private readonly UserFactory _userFactory;
    private readonly List<User> _userPool;

    public GetUserApiTest(AppFactory<Program, SafariDigitalContext> fixture) : base(fixture)
    {
        Repository<User> userRepository = new(GetContext());
        _userFactory = new UserFactory(userRepository);
        _userPool = Setup();
    }

    [Fact]
    public async Task GetUser_ReturnsRows()
    {
        var response = await BaseClient.GetUsers(new UserQuery());
        var content = await response.Content.ReadFromJsonAsync<QueryResult<UserModel>>();
        Assert.Equal(_userPool.Count + 1, content?.Count);
        _userFactory.Dispose();
    }

    [Fact]
    private async Task GetUser_ReturnsRows_WhenQuerySpecificUsername()
    {
        var response = await BaseClient.GetUsers(new UserQuery { Username = "user1" });
        var content = await response.Content.ReadFromJsonAsync<QueryResult<UserModel>>();
        Assert.Equal(11, content?.Count);
        _userFactory.Dispose();
    }

    [Fact]
    private async Task GetUser_ReturnsRows_WhenQueryMultipleFilters()
    {
        var response = await BaseClient.GetUsers(new UserQuery { Username = "user4", IsActive = false });
        var content = await response.Content.ReadFromJsonAsync<QueryResult<UserModel>>();
        Assert.Equal(1, content?.Count);
        _userFactory.Dispose();
    }

    [Fact]
    public async Task GetUserById_ReturnsUser()
    {
        var response = await BaseClient.GetUser(_userPool[0].Id);
        var content = await response.Content.ReadFromJsonAsync<Result<UserModel>>();
        Assert.Equal(_userPool[0].Id, content?.Value?.Id);
        _userFactory.Dispose();
    }

    private List<User> Setup()
    {
        var (user, password) = _userFactory.CreateUser(new UserPayload
        {
            Password = Randomizer.GenerateRandomString(),
            Role = UserRole.Admin
        });
        BaseClient.Login(user.Login, password).Wait();

        List<User> users = [];
        for (var i = 0; i < 25; i++)
        {
            var (usr, _) = _userFactory.CreateUser(new UserPayload
            {
                Username = $"user{i}",
                Email = $"user{i}@msn.com",
                Role = i is >= 10 and <= 20 ? UserRole.Admin : i > 20 ? UserRole.SuperAdmin : UserRole.User,
                IsActive = i > 5
            });
            users.Add(usr);
        }

        return users;
    }
}