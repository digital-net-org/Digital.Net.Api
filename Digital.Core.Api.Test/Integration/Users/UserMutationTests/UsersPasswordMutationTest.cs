using System.Net;
using Digital.Core.Api.Test.Api;
using Digital.Core.Api.Test.Utils;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Users.UserMutationTests;

public class UsersPasswordMutationTest(AppFactory<Program> fixture) : UsersTest(fixture)
{
    private const string ValidPassword = "12345newShinyPassword!!";
    private const string InvalidPassword = "not a valid password";

    [Fact]
    public async Task UpdatePassword_WithValidPassword_ShouldReturnOk() =>
        await ExecuteTest(DataFactory.TestUserPassword, ValidPassword, HttpStatusCode.OK);

    [Fact]
    public async Task UpdatePassword_WithInvalidCredentials_ShouldReturnUnauthorized() =>
        await ExecuteTest(InvalidPassword, ValidPassword, HttpStatusCode.Unauthorized);

    [Fact]
    public async Task UpdatePassword_WithInvalidPassword_ShouldReturnUnauthorized() =>
        await ExecuteTest(DataFactory.TestUserPassword, InvalidPassword, HttpStatusCode.BadRequest);

    private async Task ExecuteTest(string password, string payload, HttpStatusCode expectedStatusCode)
    {
        var expectSuccess = expectedStatusCode == HttpStatusCode.OK;
        var user = GetUser();
        await BaseClient.Login(user);
        
        var initialPassword = user.Password;
        var mutationResult = (await BaseClient.UpdatePassword(user.Id, password, payload)).StatusCode;
        await UserRepository.ReloadAsync(user);
        var mutatedPassword = user.Password;
        var loginResult = (await BaseClient.Login(user.Login, payload)).StatusCode;

        Assert.Equal(expectedStatusCode, mutationResult);
        if (expectSuccess)
        {
            Assert.NotEqual(initialPassword, mutatedPassword);
            Assert.Equal(HttpStatusCode.OK, loginResult);
        }
        else
        {
            Assert.Equal(initialPassword, mutatedPassword);
            Assert.Equal(HttpStatusCode.Unauthorized, loginResult);
        }
    }
}