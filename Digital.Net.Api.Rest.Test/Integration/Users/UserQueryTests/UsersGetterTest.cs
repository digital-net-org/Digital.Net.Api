using System.Net;
using System.Net.Http.Json;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Rest.Test.Api;
using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.Rest.Test.Integration.Users.UserQueryTests;

public class UsersGetterTest(AppFactory<Program> fixture) : UsersTest(fixture)
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