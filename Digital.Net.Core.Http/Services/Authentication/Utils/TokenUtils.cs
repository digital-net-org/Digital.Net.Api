using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;

namespace Digital.Net.Core.Http.Services.Authentication.Utils;

public static class TokenUtils
{
    public static TokenType? GetTokenType(this JwtSecurityToken jwt) =>
        jwt.Claims.FirstOrDefault(c => c.Type == AuthenticationStaticOptions.TokenTypeClaimType)?.Value is { } value
        && Enum.TryParse<TokenType>(value, out var type)
            ? type
            : null;

    public static TokenContent Decode(this JwtSecurityToken jwt)
    {
        var content = jwt.Claims.First(c => c.Type == AuthenticationStaticOptions.ContentClaimType).Value;
        return JsonSerializer.Deserialize<TokenContent>(content) ?? throw new InvalidTokenException();
    }
}