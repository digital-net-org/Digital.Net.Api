using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Services.Application;

public static class ApplicationName
{
    private const string ApplicationNameAccessor = "ApplicationName";

    public static WebApplicationBuilder SetApplicationName(this WebApplicationBuilder builder, string applicationName)
    {
        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string> { { ApplicationNameAccessor, applicationName } }!
        );
        return builder;
    }

    public static string GetApplicationName(this IConfiguration configuration) =>
        configuration[ApplicationNameAccessor] ?? throw new Exception("ApplicationName not set.");

    public static string GetApplicationName(this WebApplicationBuilder builder) =>
        builder.Configuration.GetApplicationName();
}