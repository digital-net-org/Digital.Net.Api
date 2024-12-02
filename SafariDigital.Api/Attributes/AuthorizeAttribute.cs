using Digital.Net.Mvc.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Services.Authentication.Jwt;
using SafariDigital.Services.Authentication.Service;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Api.Attributes;

[AttributeUsage(AttributeTargets.All)]
public class AuthorizeAttribute : Attribute, IAuthorizationFilter
{
    public EUserRole Role { get; set; }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var jwtService = context.HttpContext.RequestServices.GetRequiredService<IJwtService>();
        var httpContextService = context.HttpContext.RequestServices.GetRequiredService<IHttpContextService>();

        var result = jwtService.ValidateBearerToken(httpContextService.BearerToken);
        var isUserAuthorized = result.Value?.Role >= Role;

        if (isUserAuthorized)
        {
            httpContextService.AddItem(
                AuthenticatedUserService.Token,
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