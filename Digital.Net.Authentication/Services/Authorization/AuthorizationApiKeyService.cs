using Digital.Net.Authentication.Exceptions;
using Digital.Net.Authentication.Models;
using Digital.Net.Authentication.Options;
using Digital.Net.Core.Http;
using Digital.Net.Entities.Models.ApiKeys;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Repositories;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Authentication.Services.Authorization;

public class AuthorizationApiKeyService(
    IRepository<ApiKey> apiKeyRepository,
    IRepository<User> userRepository,
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

        var authorization = apiKeyRepository.Get(k => k.Key == ApiKey.Hash(key)).FirstOrDefault();
        if (authorization is null)
            return result.AddError(new InvalidTokenException());

        if (authorization.ExpiredAt is not null && authorization.ExpiredAt < DateTime.UtcNow)
            return result.AddError(new ExpiredTokenException());

        var user = userRepository.Get(u => u.Id == authorization.UserId && u.IsActive).FirstOrDefault();
        if (user is null)
            return result.AddError(new InvalidTokenException());

        result.Authorize(user.Id);
        return result;
    }
}