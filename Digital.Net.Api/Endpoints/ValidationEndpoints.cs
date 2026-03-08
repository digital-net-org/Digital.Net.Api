using Digital.Net.Api.RateLimiter.Limiters;
using Digital.Net.Api.Services.Authentication.Filters;
using Digital.Net.Core.Settings;
using Digital.Net.Core.String;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Endpoints;

public static class ValidationEndpoints
{
    public static IEndpointRouteBuilder MapValidationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/validation")
            .WithTags("Validation")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("/pattern/email", () => TypedResults.Ok(RegularExpressions.EmailPattern))
            .WithSummary("GetEmailPattern")
            .WithDescription("Get the email pattern as a string. This is designed to validate email addresses.");

        controller
            .MapGet("/pattern/username", () => TypedResults.Ok(RegularExpressions.UsernamePattern))
            .RequireAuthentication(AuthorizeType.Any)
            .WithSummary("GetUsernamePattern")
            .WithDescription("Get the username pattern as a string. This is designed to validate usernames.");

        controller
            .MapGet("/pattern/password", () => TypedResults.Ok(RegularExpressions.PasswordPattern))
            .RequireAuthentication(AuthorizeType.Any)
            .WithSummary("GetPasswordPattern")
            .WithDescription("Get the password pattern as a string. This is designed to validate passwords.");

        controller
            .MapGet("/size/avatar", () => TypedResults.Ok(AppSettings.DefaultMaxAvatarSize))
            .RequireAuthentication(AuthorizeType.Any)
            .WithSummary("GetAvatarSizeLimit")
            .WithDescription("Get the maximum allowed size for avatar images in bytes.");

        return app;
    }
}