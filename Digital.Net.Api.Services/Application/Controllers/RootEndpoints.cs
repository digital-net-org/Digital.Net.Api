using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Services.Application.Controllers.Dto;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;

namespace Digital.Net.Api.Services.Application.Controllers;

public static class RootEndpoints
{
    public static IEndpointRouteBuilder MapRootEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/")
            .WithTags("Application");

        controller
            .MapGet("/", GetApplicationVersion)
            .AddOpenApiOperationTransformer((operation, _, _) =>
            {
                operation.Summary = "Get the application version.";
                return Task.CompletedTask;
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