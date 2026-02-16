using Digital.Net.Api.Authentication.Exceptions;
using Digital.Net.Api.Authentication.Models;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Core.Http;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Authentication.Services.Authorization;

public class AuthorizationApiKeyService(
    IRepository<ApiKey, DigitalContext> apiKeyRepository,
    IRepository<User, DigitalContext> userRepository,
    IHttpContextAccessor httpContextAccessor,
    IAuthenticationOptionService optionService
) : IAuthorizationApiKeyService
{
    public string? GetRequestKey() =>
        httpContextAccessor.GetRequest().Headers[optionService.ApiKeyHeaderAccessor].FirstOrDefault();

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

        var user = userRepository.Get(u => u.Id == authorization.UserId).FirstOrDefault();
        if (user is null)
            return result.AddError(new InvalidTokenException());

        result.Authorize(user.Id);
        return result;
    }
}