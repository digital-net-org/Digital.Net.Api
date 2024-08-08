using Microsoft.AspNetCore.Http;
using Safari.Net.Data.Repositories;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Services.HttpContextService;

public class HttpContextService(
    IHttpContextAccessor contextAccessor,
    IRepository<User> userRepository) : IHttpContextService
{
    public (HttpRequest request, HttpResponse response) GetControllerContext() => (GetRequest(), GetResponse());

    public async Task<User> GetAuthenticatedUser()
    {
        var context = GetContext();
        var token = context.GetTokenFromContext();
        var user = await userRepository.GetByIdAsync(token?.Id);
        return user ?? throw new NullReferenceException("No user authenticated");
    }

    public HttpContext GetContext() =>
        contextAccessor.HttpContext ?? throw new NullReferenceException("Http Context is not defined");

    private HttpRequest GetRequest() =>
        GetContext().Request ?? throw new NullReferenceException("Http Request is not defined");

    private HttpResponse GetResponse() =>
        GetContext().Response ?? throw new NullReferenceException("Http Response is not defined");
}