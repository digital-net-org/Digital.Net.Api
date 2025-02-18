using System.Net;
using Digital.Core.Api.Test.TestUtilities;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.ApiKeys;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.TestTools.Integration;

namespace Digital.Core.Api.Test.Integration.AuthenticationApi;

public class ApiKeyIntegrationTest : IntegrationTest<Program, DigitalContext>
{
    private readonly Repository<User, DigitalContext> _userRepository;
    private readonly Repository<ApiKey, DigitalContext> _apiKeyRepository;

    public ApiKeyIntegrationTest(AppFactory<Program, DigitalContext> fixture) : base(fixture)
    {
        var context = GetService<DigitalContext>();
        _apiKeyRepository = new Repository<ApiKey, DigitalContext>(context);
        _userRepository = new Repository<User, DigitalContext>(context);
    }

    private void Setup(DateTime? expiredAt = null)
    {
        var apiKey = _apiKeyRepository.Get().First();
        BaseClient.DefaultRequestHeaders.Add("domain.test_auth", apiKey.Key);
    }

    [Fact]
    public async Task Authorize_WithValidApiKey_ShouldReturnOk()
    {
        Setup();
        var response = await BaseClient.TestUserAuthorization();
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Authorize_ShouldReturnUnauthorized_OnMissingApiKeyHeader()
    {
        var response = await BaseClient.TestUserAuthorization();
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}