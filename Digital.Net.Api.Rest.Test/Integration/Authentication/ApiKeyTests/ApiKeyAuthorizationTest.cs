using System.Net;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Api.Rest.Test.Api;
using Digital.Net.Api.TestUtilities.Data.Factories;
using Digital.Net.Api.TestUtilities.Integration;

namespace Digital.Net.Api.Rest.Test.Integration.Authentication.ApiKeyTests;

public class ApiKeyAuthorizationTest(AppFactory<Program> fixture) : AuthenticationTest(fixture)
{
    [Fact]
    public async Task Authorize_WithValidApiKey_ShouldReturnOk()
    {
        await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128));
        await ExecuteTest(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Authorize_ShouldReturnUnauthorized_OnExpiredApiKey()
    {
        await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128), DateTime.UtcNow.AddDays(-7));
        await ExecuteTest(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidHeader()
    {
        await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128), header: "Invalid");
        await ExecuteTest(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidApiKey()
    {
        BaseClient.DefaultRequestHeaders.Add(HeaderKey, "SomeString");
        await ExecuteTest(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingApiKeyHeader() =>
        await ExecuteTest(HttpStatusCode.Unauthorized);

    private async Task Setup(string key, DateTime? expiry = null, string? header = null)
    {
        var user = UserRepository.BuildTestUser();
        await ApiKeyRepository.CreateAndSaveAsync(new ApiKey(user.Id, key, expiry));
        BaseClient.DefaultRequestHeaders.Add(header ?? HeaderKey, key);
    }

    private async Task ExecuteTest(HttpStatusCode expectedResult)
    {
        var response = await BaseClient.GetUsers(new UserQuery());
        Assert.Equal(expectedResult, response.StatusCode);
    }
}
