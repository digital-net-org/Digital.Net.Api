using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.App.Test.Integration.Users.UserQueryTests;

public class UsersGetterTest(AppFactory<Program> fixture) : UsersTest(fixture)
{
    [Fact]
    public async Task GetUserById_ReturnsUser() => throw
        // await BaseClient.Login(UserRepository.Get().First());
        // var payload = UserPool.Skip(5).First().Id;
        // var response = await BaseClient.GetUser(payload);
        // var content = await response.Content.ReadFromJsonAsync<Result<UserDto>>();
        // Assert.Equal(payload, content?.Value?.Id);
        // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        new NotImplementedException();
}