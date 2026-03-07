using System.IdentityModel.Tokens.Jwt;
using Digital.Net.Authentication.Exceptions;
using Digital.Net.Authentication.Models;
using Digital.Net.Authentication.Options;
using Digital.Net.Authentication.Services.AuthContext;
using Digital.Net.Entities.Models.Users;
using Digital.Net.Entities.Context;

namespace Digital.Net.Authentication.Services.Authorization;

public class AuthorizationJwtService(
    DigitalContext context,
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
            var user = context.Users.FirstOrDefault(u => u.Id == decoded.Id && u.IsActive);

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
        var record = context.ApiTokens.FirstOrDefault(a => a.Key == (token ?? string.Empty));
        return record is null
            ? new AuthorizationResult().AddError(new InvalidTokenException())
            : AuthorizeUser(record.Key);
    }
}