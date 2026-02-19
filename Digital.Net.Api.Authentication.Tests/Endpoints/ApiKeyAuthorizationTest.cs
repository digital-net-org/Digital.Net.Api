using System.Net;
using Digital.Net.Api.Controllers.Controllers.UserApi.Dto;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Authentication.Tests.Endpoints;

public class ApiKeyAuthorizationTest : AuthenticationTest
{
    [Test]
    public async Task Authorize_WithValidApiKey_ShouldReturnOk()
    {
        await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128));
        await ExecuteTestAsync(HttpStatusCode.OK);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnExpiredApiKey()
    {
        await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128), DateTime.UtcNow.AddDays(-7));
        await ExecuteTestAsync(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidHeader()
    {
        await Setup(Randomizer.GenerateRandomString(Randomizer.AnyLetter, 128), header: "Invalid");
        await ExecuteTestAsync(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnInvalidApiKey()
    {
        var client = Application.CreateClient();
        client.DefaultRequestHeaders.Add(HeaderKey, "SomeString");
        await ExecuteTestAsync(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingApiKeyHeader() =>
        await ExecuteTestAsync(HttpStatusCode.Unauthorized);

    private async Task Setup(string key, DateTime? expiry = null, string? header = null)
    {
        var client = Application.CreateClient();
        var user = Application.CreateUser();
        await Application
            .GetRepository<ApiKey>()
            .CreateAndSaveAsync(new ApiKey(user.Id, key, expiry));

        client.DefaultRequestHeaders.Add(header ?? HeaderKey, key);
    }

    private async Task ExecuteTestAsync(HttpStatusCode expectedResult)
    {
        var client = Application.CreateClient();
        var response = await client.GetUsers(new UserQuery());
        await Assert.That(response.StatusCode).EqualTo(expectedResult);
    }
}