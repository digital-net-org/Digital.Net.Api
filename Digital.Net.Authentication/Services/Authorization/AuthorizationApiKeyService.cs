using Digital.Net.Authentication.Exceptions;
using Digital.Net.Authentication.Models;
using Digital.Net.Authentication.Options;
using Digital.Net.Core.Http;
using Digital.Net.Entities.Models.ApiKeys;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Context;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Authentication.Services.Authorization;

public class AuthorizationApiKeyService(
    DigitalContext context,
    IHttpContextAccessor httpContextAccessor
) : IAuthorizationApiKeyService
{
    public string? GetRequestKey() =>
        httpContextAccessor.GetRequest().Headers[AuthenticationStaticOptions.ApiKeyHeaderAccessor].FirstOrDefault();

    public AuthorizationResult AuthorizeUser(string? key)
    {
        var result = new AuthorizationResult();
        if (string.IsNullOrWhiteSpace(key))
            return result.AddError(new TokenNotFoundException());

        var authorization = context.ApiKeys.FirstOrDefault(k => k.Key == ApiKey.Hash(key));
        if (authorization is null)
            return result.AddError(new InvalidTokenException());

        if (authorization.ExpiredAt is not null && authorization.ExpiredAt < DateTime.UtcNow)
            return result.AddError(new ExpiredTokenException());

        var user = context.Users.FirstOrDefault(u => u.Id == authorization.UserId && u.IsActive);
        if (user is null)
            return result.AddError(new InvalidTokenException());

        result.Authorize(user.Id);
        return result;
    }
}