using System;
using System.Threading.Tasks;

namespace Digital.Net.Tests.Program.TESTS_TO_MIGRATE.UserQueryTests;

public class UsersGetterTest : UsersTest
{
    public async Task GetUserById_ReturnsUser() => throw
        // await BaseClient.Login(UserRepository.Get().First());
        // var payload = UserPool.Skip(5).First().Id;
        // var response = await BaseClient.GetUser(payload);
        // var content = await response.Content.ReadFromJsonAsync<Result<UserDto>>();
        // Assert.Equal(payload, content?.Value?.Id);
        // Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        new NotImplementedException();
}