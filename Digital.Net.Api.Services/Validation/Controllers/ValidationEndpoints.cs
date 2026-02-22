using Digital.Net.Api.Authentication.Filters;
using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.Core.Settings;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Services.Validation.Controllers;

public static class ValidationEndpoints
{
    public static IEndpointRouteBuilder MapValidationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/validation")
            .WithTags("Validation");

        controller
            .MapGet("/pattern/email", () => Results.Ok(RegularExpressions.EmailPattern));

        controller
            .MapGet("/pattern/username", () => Results.Ok(RegularExpressions.UsernamePattern))
            .RequireAuthentication(AuthorizeType.Any);

        controller
            .MapGet("/pattern/password", () => Results.Ok(RegularExpressions.PasswordPattern))
            .RequireAuthentication(AuthorizeType.Any);

        controller
            .MapGet("/size/avatar", () => Results.Ok(AppSettings.DefaultMaxAvatarSize))
            .RequireAuthentication(AuthorizeType.Any);

        return app;
    }
}