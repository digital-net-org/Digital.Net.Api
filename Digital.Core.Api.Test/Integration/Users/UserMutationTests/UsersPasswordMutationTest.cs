using System.Net;
using Digital.Core.Api.Test.Api;
using Digital.Core.Api.Test.Utils;
using Digital.Lib.Net.Authentication.Exceptions;
using Digital.Lib.Net.Authentication.Services.Authentication;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Users.UserMutationTests;

public class UsersPasswordMutationTest : UsersTest
{
    private readonly IAuthenticationService _authenticationService;

    public UsersPasswordMutationTest(AppFactory<Program> fixture) : base(fixture)
    {
        _authenticationService = GetService<IAuthenticationService>();
    }

    private const string ValidPassword = "12345newShinyPassword!!";
    private const string InvalidPassword = "not a valid password";

    [Fact]
    public async Task UpdatePassword_WithValidPassword_ShouldReturnOk() =>
        await ExecuteTest(TestUserFactory.TestUserPassword, ValidPassword, HttpStatusCode.OK);

    [Fact]
    public async Task UpdatePassword_WithInvalidCredentials_ShouldReturnUnauthorized() =>
        await ExecuteTest(InvalidPassword, ValidPassword, HttpStatusCode.Unauthorized);

    [Fact]
    public async Task UpdatePassword_WithInvalidPassword_ShouldReturnUnauthorized() =>
        await ExecuteTest(TestUserFactory.TestUserPassword, InvalidPassword, HttpStatusCode.BadRequest);

    private async Task ExecuteTest(string password, string payload, HttpStatusCode expectedStatusCode)
    {
        var expectSuccess = expectedStatusCode == HttpStatusCode.OK;
        var user = GetLoggedUser();
        
        var initialPassword = user.Password;
        var mutationResult = (await BaseClient.UpdatePassword(user.Id, password, payload)).StatusCode;
        await UserRepository.ReloadAsync(user);
        var mutatedPassword = user.Password;

        var credentialsTest = await _authenticationService.ValidateCredentialsAsync(user.Login, payload);

        Assert.Equal(expectedStatusCode, mutationResult);
        if (expectSuccess)
        {
            Assert.NotEqual(initialPassword, mutatedPassword);
            Assert.False(credentialsTest.HasError());
        }
        else
        {
            Assert.Equal(initialPassword, mutatedPassword);
            Assert.True(credentialsTest.HasError<InvalidCredentialsException>());
        }
    }

    private User GetLoggedUser()
    {
        var user = GetUser();
        SetAsLogged(BaseClient, user);
        return user;
    }
}