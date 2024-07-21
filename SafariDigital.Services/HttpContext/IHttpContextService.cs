using Microsoft.AspNetCore.Http;
using SafariDigital.Database.Models.UserTable;

namespace SafariDigital.Services.HttpContext;

public interface IHttpContextService
{
    (HttpRequest request, HttpResponse response) GetControllerContext();
    Microsoft.AspNetCore.Http.HttpContext GetContext();
    Task<User> GetAuthenticatedUser();
}