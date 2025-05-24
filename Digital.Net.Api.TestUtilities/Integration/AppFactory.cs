using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Digital.Net.Api.TestUtilities.Integration;

public class AppFactory<T> : WebApplicationFactory<T> where T : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .UseTestEnvironment()
            .UseTestConfiguration();
}