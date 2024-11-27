using Digital.Net.Core.Extensions.HttpUtilities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Services.HttpContext;
using SafariDigital.Services.Jwt;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Api.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public EUserRole Role { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        var token = context.HttpContext.Request.GetBearerToken();
        var result = jwtService.ValidateBearerToken(token);
        var isUserAuthorized = result.Value?.Role >= Role;

        if (isUserAuthorized)
        {
            context.HttpContext.AddItem(
                HttpContextService.Token,
                new AuthenticatedUser
                {
                    Id = result.Value?.Id,
                    Role = result.Value?.Role,
                    Token = result.Token
                }
            );
            return;
        }

        context.Result = new JsonResult(result) { StatusCode = 401 };
    }
}