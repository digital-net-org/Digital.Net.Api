using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using SafariDigital.Api;
using Tests.Core.Base;
using Xunit;

namespace Tests.Core.Integration;

public abstract class IntegrationTest : UnitTest, IClassFixture<ApiFactory<Program>>
{
    protected readonly HttpClient Client;
    protected readonly IConfiguration Configuration;
    protected readonly WebApplicationFactory<Program> Factory;

    protected IntegrationTest(ApiFactory<Program> fixture)
    {
        Factory = fixture;
        Client = Factory.CreateClient();
        Configuration = new ConfigurationBuilder().AddJsonFile("appsettings.Test.json").Build();
    }
}