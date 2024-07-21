using Microsoft.AspNetCore.Http;
using SafariDigital.Database.Models.UserTable;

namespace SafariDigital.Services.HttpContextService;

public class HttpContextService(IHttpContextAccessor contextAccessor) : IHttpContextService
{
    public (HttpRequest request, HttpResponse response) GetControllerContext() => (GetRequest(), GetResponse());


    public Task<User> GetAuthenticatedUser() => throw new NotImplementedException();

    public Microsoft.AspNetCore.Http.HttpContext GetContext() =>
        contextAccessor.HttpContext ?? throw new NullReferenceException("Http Context is not defined");

    private HttpRequest GetRequest() =>
        GetContext().Request ?? throw new NullReferenceException("Http Request is not defined");

    private HttpResponse GetResponse() =>
        GetContext().Response ?? throw new NullReferenceException("Http Response is not defined");
}