using Digital.Net.Controllers.Dto;
using Digital.Net.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Controllers.Controllers;

public static class RootEndpoints
{
    public static IEndpointRouteBuilder MapRootEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/")
            .WithTags("Application");

        controller
            .MapGet("/", GetApplicationVersion)
            .WithSummary("Gets application version")
            .WithDescription("Get the application version information.");

        return app;
    }

    private static Ok<ApplicationVersion> GetApplicationVersion(IConfiguration configuration) => TypedResults.Ok(
        new ApplicationVersion(
            "TestProgram",
            "Digital.Net",
            configuration[AppSettings.GitOrigin] ?? string.Empty,
            configuration[AppSettings.GitCommitSha] ?? string.Empty,
            configuration[AppSettings.GitRelease] ?? string.Empty
        ));
}