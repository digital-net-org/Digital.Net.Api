using System.Threading.Tasks;
using Digital.Net.Cms;
using Digital.Net.Cms.Http;
using Digital.Net.Core;
using Digital.Net.Core.Http;
using Digital.Net.Core.Http.Services.Authentication.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Tests.Program;

public sealed class DigitalProgram
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalNetCore()
            .AddDigitalNetCoreHttp()
            .AddDigitalNetCms()
            .AddDigitalNetCmsHttp()
            .Build();

        app.UseDigitalNetCoreHttp();
        app.UseDigitalNetCmsHttp();
        MapTestEndpoints(app);


        await app.RunAsync();
    }

    private static IEndpointRouteBuilder MapTestEndpoints(IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/test/authentication")
            .WithTags("Test");

        controller
            .MapGet("/session", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Session);

        controller
            .MapGet("/api-key", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.ApiKey);

        controller
            .MapGet("/any", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Session | AuthorizeType.ApiKey);

        // Mutating counterparts, so the CSRF check can be exercised per scheme.
        controller
            .MapMethods("/session-mutation", ["POST", "PUT", "PATCH", "DELETE"], () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Session);

        controller
            .MapPost("/api-key-mutation", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.ApiKey);

        controller
            .MapPost("/application-mutation", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Application);

        controller
            .MapGet("/application", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Application | AuthorizeType.Session | AuthorizeType.ApiKey);

        controller
            .MapGet("/admin", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Session | AuthorizeType.ApiKey)
            .RequireAdmin();

        return app;
    }
}