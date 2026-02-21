using Digital.Net.Api.Authentication.Filters;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Tests.Program;

public static class TestRouter
{
    public static void MapTestEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app.MapGroup("test/authentication");

        controller.MapGet("jwt", () => Results.Ok()).RequireAuthentication(AuthorizeType.Jwt);
        controller.MapGet("api-key", () => Results.Ok()).RequireAuthentication(AuthorizeType.ApiKey);
        controller.MapGet("any", () => Results.Ok()).RequireAuthentication(AuthorizeType.Any);
    }
}