using Digital.Net.Core.Http.Endpoints.Dto;
using Digital.Net.Core.Http.Security;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Core.Http.Endpoints;

public static class RootEndpoints
{
    public static IEndpointRouteBuilder MapRootEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/")
            .WithTags("Application")
            .RequireRateLimiting(RateLimiter.Policy);

        controller
            .MapGet("/", GetApplicationVersion)
            .WithSummary("Gets application version")
            .WithDescription("Get the application version information.");

        controller
            .MapGet("/ping", () => "pong")
            .WithSummary("Application health-check")
            .WithDescription("Verify if the application is up and running.");

        return app;
    }

    private static Ok<Result<ApplicationVersion>> GetApplicationVersion(IConfiguration configuration) =>
        TypedResults.Ok(
            new Result<ApplicationVersion>(new ApplicationVersion(
                configuration[CoreSettings.ApplicationNameKey] ?? string.Empty,
                "Digital.Net",
                configuration[CoreSettings.GitBranchKey] ?? string.Empty,
                configuration[CoreSettings.GitTagKey] ?? string.Empty,
                configuration[CoreSettings.GitCommitKey] ?? string.Empty
            )));
}