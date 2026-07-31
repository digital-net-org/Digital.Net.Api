using Digital.Net.Core;
using Digital.Net.Core.Http.Bootstrap;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Tests.Core.Http.Bootstrap;

public class CorsPolicyInjectorTest
{
    [Test]
    public async Task AddDefaultCorsPolicy_WithoutAnyOrigin_ShouldThrow()
    {
        var builder = BuildWith(null);

        await Assert.That(() => builder.AddDefaultCorsPolicy()).Throws<InvalidOperationException>();
    }

    [Test]
    public async Task AddDefaultCorsPolicy_WithAnOrigin_ShouldSucceed()
    {
        var builder = BuildWith("https://bo.domain.test");

        await Assert.That(() => builder.AddDefaultCorsPolicy()).ThrowsNothing();
    }

    private static WebApplicationBuilder BuildWith(string? origin)
    {
        var builder = WebApplication.CreateBuilder();
        // Drop the ambient appsettings so the test states the whole configuration itself.
        builder.Configuration.Sources.Clear();
        if (origin is not null)
            builder.Configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { [$"{CoreSettings.CorsAllowedOriginsKey}:0"] = origin });
        return builder;
    }
}