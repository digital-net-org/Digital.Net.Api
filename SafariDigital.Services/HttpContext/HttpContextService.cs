using Digital.Net.Core.Extensions.HttpUtilities;
using Digital.Net.Entities.Repositories;
using Microsoft.AspNetCore.Http;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.HttpContext;

public class HttpContextService(
    IHttpContextAccessor contextAccessor,
    IRepository<User> userRepository) : IHttpContextService
{
    public const string Token = "Token";
    public HttpRequest Request => contextAccessor.GetRequest();
    public HttpResponse Response => contextAccessor.GetResponse();

    public async Task<User> GetAuthenticatedUser()
    {
        var context = contextAccessor.GetContext();
        var token = context.GetItem<AuthenticatedUser>(Token);
        var user = await userRepository.GetByIdAsync(token?.Id);
        return user ?? throw new NullReferenceException("No user authenticated");
    }
}