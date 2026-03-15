using Digital.Net.Core.Endpoints.Dto;
using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Lib.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Core.Endpoints;

public static class RootEndpoints
{
    public static IEndpointRouteBuilder MapRootEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/")
            .WithTags("Application")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("/", GetApplicationVersion)
            .WithSummary("Gets application version")
            .WithDescription("Get the application version information.");

        return app;
    }

    private static Ok<ApplicationVersion> GetApplicationVersion(IConfiguration configuration) => TypedResults.Ok(
        new ApplicationVersion(
            configuration[AppSettings.ApplicationNameKey] ?? string.Empty,
            "Digital.Net",
            configuration[AppSettings.GitOriginKey] ?? string.Empty,
            configuration[AppSettings.GitCommitShaKey] ?? string.Empty,
            configuration[AppSettings.GitReleaseKey] ?? string.Empty
        ));
}