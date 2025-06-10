using System.IdentityModel.Tokens.Jwt;
using System.Text.Json;
using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.Services.Authentication.Services.Authorization;

namespace Digital.Net.Api.Services.Authentication.Services;

public static class TokenUtils
{
    /// <summary>
    ///     Verifies if the refresh token should be refreshed based on its lifetime.
    ///     If the token is less than 20% of its total lifetime or has more than 1 day left, it should be refreshed.
    /// </summary>
    /// <returns>A boolean indicating whether the token should be refreshed.</returns>
    public static bool ShouldRenewCookie(this JwtSecurityToken token)
    {
        var totalLifetime = token.ValidTo - token.IssuedAt;
        var timeLeft = token.ValidTo - DateTime.UtcNow;

        if (timeLeft <= TimeSpan.Zero)
            throw new InvalidTokenException();

        var percentLeft = timeLeft.TotalSeconds / totalLifetime.TotalSeconds;
        return percentLeft < 0.2 || timeLeft > TimeSpan.FromDays(1);
    }
    
    public static TokenContent Decode(this JwtSecurityToken jwt)
    {
        var content = jwt.Claims.First(c => c.Type == DefaultAuthenticationOptions.ContentClaimType).Value;
        return JsonSerializer.Deserialize<TokenContent>(content) ?? throw new InvalidTokenException();
    }
}