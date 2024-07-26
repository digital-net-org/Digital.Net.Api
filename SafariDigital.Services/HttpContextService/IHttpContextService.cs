using Microsoft.AspNetCore.Http;
using SafariDigital.Database.Models.UserTable;

namespace SafariDigital.Services.HttpContextService;

public interface IHttpContextService
{
    (HttpRequest request, HttpResponse response) GetControllerContext();
    HttpContext GetContext();
    Task<User> GetAuthenticatedUser();
}