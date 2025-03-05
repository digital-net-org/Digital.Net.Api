using System.Net;
using Digital.Core.Api.Test.Collections;
using Digital.Lib.Net.Core.Random;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.ApiKeys;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.Authentication;

public class ApiKeyIntegrationTest : IntegrationTest<Program>
{
    private readonly string _header = $"{AppFactorySettings.TestSettings["Domain"] ?? throw new Exception()}_auth";
    private readonly IRepository<User, DigitalContext> _userRepository;
    private readonly IRepository<ApiKey, DigitalContext> _apiKeyRepository;

    public ApiKeyIntegrationTest(AppFactory<Program> fixture) : base(fixture)
    {
        _userRepository = GetRepository<User, DigitalContext>();
        _apiKeyRepository = GetRepository<ApiKey, DigitalContext>();
    }

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
        BaseClient.DefaultRequestHeaders.Add(_header, "SomeString");
        await ExecuteTest(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingApiKeyHeader() =>
        await ExecuteTest(HttpStatusCode.Unauthorized);

    private async Task Setup(string key, DateTime? expiry = null, string? header = null)
    {
        var user = _userRepository.Get().First();
        await _apiKeyRepository.CreateAndSaveAsync(new ApiKey(user.Id, key, expiry));
        BaseClient.DefaultRequestHeaders.Add(header ?? _header, key);
    }

    private async Task ExecuteTest(HttpStatusCode expectedResult)
    {
        var response = await BaseClient.GetAppVersion();
        Assert.Equal(expectedResult, response.StatusCode);
    }
}
