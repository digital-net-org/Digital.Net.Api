using Digital.Net.Core.Http;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Authentication.Services.AuthContext;

public class AuthContextService(
    IHttpContextAccessor contextAccessor
) : IAuthContextService
{
    public string? BearerToken =>
        contextAccessor.GetRequest().Headers.Authorization.FirstOrDefault()?.Split(" ").Last();
}