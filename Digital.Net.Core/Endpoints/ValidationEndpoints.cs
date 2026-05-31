using Digital.Net.Core.RateLimiter.Limiters;
using Digital.Net.Lib.Messages;
using Digital.Net.Core.Services.Authentication.Filters;
using Digital.Net.Lib.Settings;
using Digital.Net.Lib.String;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Core.Endpoints;

public static class ValidationEndpoints
{
    public static IEndpointRouteBuilder MapValidationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/validation")
            .WithTags("Validation")
            .RequireRateLimiting(GlobalLimiter.Policy);

        controller
            .MapGet("/pattern/email", () => TypedResults.Ok(new Result<string>(RegularExpressions.EmailPattern)))
            .WithSummary("GetEmailPattern")
            .WithDescription("Get the email pattern as a string. This is designed to validate email addresses.");

        controller
            .MapGet("/pattern/username", () => TypedResults.Ok(new Result<string>(RegularExpressions.UsernamePattern)))
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .WithSummary("GetUsernamePattern")
            .WithDescription("Get the username pattern as a string. This is designed to validate usernames.");
    
        controller
            .MapGet("/pattern/password", () => TypedResults.Ok(new Result<string>(RegularExpressions.PasswordPattern)))
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .WithSummary("GetPasswordPattern")
            .WithDescription("Get the password pattern as a string. This is designed to validate passwords.");

        controller
            .MapGet("/size/avatar", () => TypedResults.Ok(new Result<long>(AppSettings.DefaultMaxAvatarSize)))
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .WithSummary("GetAvatarSizeLimit")
            .WithDescription("Get the maximum allowed size for avatar images in bytes.");

        controller
            .MapGet("/pattern/api-key-name", () => TypedResults.Ok(new Result<string>(RegularExpressions.ApiKeyNamePattern)))
            .RequireAuthentication(AuthorizeType.Jwt | AuthorizeType.ApiKey)
            .WithSummary("GetApiKeyNamePattern")
            .WithDescription("Get the API key name pattern as a string. This is designed to validate API key names.");

        return app;
    }
}