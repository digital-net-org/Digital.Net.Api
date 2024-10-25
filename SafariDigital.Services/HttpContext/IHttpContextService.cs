using Microsoft.AspNetCore.Http;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Services.HttpContext;

public interface IHttpContextService
{
    HttpRequest Request { get; }
    HttpResponse Response { get; }
    Task<User> GetAuthenticatedUser();
}