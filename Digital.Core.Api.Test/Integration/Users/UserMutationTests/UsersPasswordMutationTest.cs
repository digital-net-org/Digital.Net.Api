using System.Net;
using System.Net.Http.Json;
using Digital.Core.Api.Test.Api;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Users.UserMutationTests;

public class UsersPasswordMutationTest(AppFactory<Program> fixture) : UsersTest(fixture)
{
    [Fact]
    public async Task GetUserById_ReturnsUser()
    {
        await BaseClient.Login(UserRepository.Get().First());
        var payload = UserPool.Skip(5).First().Id;
        var response = await BaseClient.GetUser(payload);
        var content = await response.Content.ReadFromJsonAsync<Result<UserDto>>();
        Assert.Equal(payload, content?.Value?.Id);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}