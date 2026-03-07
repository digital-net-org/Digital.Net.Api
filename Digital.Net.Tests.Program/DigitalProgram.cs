using System.Threading.Tasks;
using Digital.Net.Authentication.Filters;
using Digital.Net.Sdk;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Tests.Program;

public sealed class DigitalProgram
{
    private static async Task Main(string[] args)
    {
        var app = WebApplication.CreateBuilder(args)
            .AddDigitalSdk()
            .Build();

        app.UseDigitalSdk();
        MapTestEndpoints(app);


        await app.RunAsync();
    }

    public static IEndpointRouteBuilder MapTestEndpoints(IEndpointRouteBuilder app)
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
            .MapGet("/admin", () => Results.Ok())
            .RequireAuthentication(AuthorizeType.Any)
            .RequireAdmin();

        return app;
    }
}