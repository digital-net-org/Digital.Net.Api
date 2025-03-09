using System.Net;
using Digital.Core.Api.Test.Api;
using Digital.Core.Api.Test.Utils;
using Digital.Lib.Net.Core.Random;
using Digital.Lib.Net.Entities.Models.ApiKeys;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication.ApiKeyTests;

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
        var response = await BaseClient.GetAppVersion();
        Assert.Equal(expectedResult, response.StatusCode);
    }
}
