using System.IdentityModel.Tokens.Jwt;
using Digital.Net.Api.Authentication.Exceptions;
using Digital.Net.Api.Authentication.Models;
using Digital.Net.Api.Authentication.Options;
using Digital.Net.Api.Authentication.Services.AuthContext;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;

namespace Digital.Net.Api.Authentication.Services.Authorization;

public class AuthorizationJwtService(
    IRepository<User> userRepository,
    IRepository<ApiToken> apiTokenRepository,
    IAuthContextService authContextService,
    IAuthenticationOptionService authenticationOptionService
) : IAuthorizationJwtService
{
    public string? GetRequestKey() => authContextService.BearerToken;

    public AuthorizationResult AuthorizeUser(string? token)
    {
        var result = new AuthorizationResult();
        if (string.IsNullOrWhiteSpace(token))
            return result.AddError(new TokenNotFoundException());

        var handler = new JwtSecurityTokenHandler();
        try
        {
            handler.ValidateToken(token, authenticationOptionService.GetTokenParameters(), out _);
            var jwt = handler.ReadJwtToken(token);
            var decoded = jwt.Decode();
            var user = userRepository.Get(u => u.Id == decoded.Id && u.IsActive).FirstOrDefault();

            if (user is null)
                throw new InvalidTokenException();

            result.Authorize(user.Id, jwt.ShouldRenewCookie());
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public AuthorizationResult AuthorizeRefreshToken(string? token)
    {
        var record = apiTokenRepository
            .Get(a => a.Key == (token ?? string.Empty))
            .FirstOrDefault();
        return record is null
            ? new AuthorizationResult().AddError(new InvalidTokenException())
            : AuthorizeUser(record.Key);
    }
}