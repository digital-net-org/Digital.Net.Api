using System.Threading.Tasks;
using Digital.Net.Api;
using Digital.Net.Api.Services.Authentication.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Tests.Program;

public sealed class DigitalProgram
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalNet()
            .Build();

        app.UseDigitalNet();
        MapTestEndpoints(app);


        await app.RunAsync();
    }

    private static IEndpointRouteBuilder MapTestEndpoints(IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/test/authentication")
            .WithTags("Test");

        controller
            .MapGet("/jwt", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Jwt);

        controller
            .MapGet("/api-key", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.ApiKey);

        controller
            .MapGet("/any", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey);

        controller
            .MapGet("/application", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Application);

        controller
            .MapGet("/admin", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .RequireAdmin();

        return app;
    }
}