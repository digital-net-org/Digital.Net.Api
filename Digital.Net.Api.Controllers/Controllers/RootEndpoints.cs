using Digital.Net.Api.Controllers.Dto;
using Digital.Net.Api.Core.OpenApi;
using Digital.Net.Api.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Controllers.Controllers;

public static class RootEndpoints
{
    public static IEndpointRouteBuilder MapRootEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/")
            .WithTags("Application");

        controller
            .MapGet("/", GetApplicationVersion)
            .WithDoc(d =>
            {
                d.Summary = "GetApplicationVersion";
                d.Description = "Get the application version information.";
            });

        return app;
    }

    private static ApplicationVersion GetApplicationVersion(IConfiguration configuration) => new(
        "Digital.Net.Api",
        configuration[AppSettings.GitOrigin] ?? string.Empty,
        configuration[AppSettings.GitCommitSha] ?? string.Empty,
        configuration[AppSettings.GitRelease] ?? string.Empty
    );
}