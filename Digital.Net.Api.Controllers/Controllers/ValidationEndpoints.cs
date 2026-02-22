using Digital.Net.Api.Authentication.Filters;
using Digital.Net.Api.Core.OpenApi;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Core.String;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Digital.Net.Api.Controllers.Controllers;

public static class ValidationEndpoints
{
    public static IEndpointRouteBuilder MapValidationEndpoints(this IEndpointRouteBuilder app)
    {
        var controller = app
            .MapGroup("/validation")
            .WithTags("Validation");

        controller
            .MapGet("/pattern/email", () => Results.Ok(RegularExpressions.EmailPattern))
            .WithDoc(d =>
            {
                d.Summary = "GetEmailPattern";
                d.Description = "Get the email pattern as a string. This is designed to validate email addresses.";
            });

        controller
            .MapGet("/pattern/username", () => Results.Ok(RegularExpressions.UsernamePattern))
            .RequireAuthentication(AuthorizeType.Any)
            .WithDoc(d =>
            {
                d.Summary = "GetUsernamePattern";
                d.Description = "Get the username pattern as a string. This is designed to validate usernames.";
            });

        controller
            .MapGet("/pattern/password", () => Results.Ok(RegularExpressions.PasswordPattern))
            .RequireAuthentication(AuthorizeType.Any)
            .WithDoc(d =>
            {
                d.Summary = "GetPasswordPattern";
                d.Description = "Get the password pattern as a string. This is designed to validate passwords.";
            });

        controller
            .MapGet("/size/avatar", () => Results.Ok(AppSettings.DefaultMaxAvatarSize))
            .RequireAuthentication(AuthorizeType.Any)
            .WithDoc(d =>
            {
                d.Summary = "GetAvatarSizeLimit";
                d.Description = "Get the maximum allowed size for avatar images in bytes.";
            });

        return app;
    }
}