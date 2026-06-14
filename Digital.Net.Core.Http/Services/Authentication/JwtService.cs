using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiTokens;
using Digital.Net.Core.Http.Services.Authentication.Exceptions;
using Digital.Net.Core.Http.Services.Authentication.Options;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Digital.Net.Core.Http.Services.Authentication.Utils;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Core.Http.Services.Authentication;

public class JwtService(
    AuthenticationOptionService authenticationOptionService,
    DigitalContext context
)
{
    public async Task RevokeTokenAsync(string token)
    {
        var record = await context.ApiTokens.FirstOrDefaultAsync(t => t.Key == ApiToken.Hash(token));
        if (record is null) return;
        context.ApiTokens.Remove(record);
        await context.SaveChangesAsync();
    }

    public async Task RevokeAllTokensAsync(Guid userId) =>
        await context.ApiTokens.Where(t => t.UserId == userId).ExecuteDeleteAsync();

    public string GenerateBearerToken(Guid userId, string userAgent)
    {
        var content = new TokenContent(userId, userAgent);
        return SignToken(content, authenticationOptionService.GetBearerTokenExpirationDate());
    }

    public async Task<string> GenerateRefreshTokenAsync(Guid userId, string userAgent, CancellationToken ct = default)
    {
        var content = new TokenContent(userId, userAgent);
        var tokenExpiration = authenticationOptionService.GetRefreshTokenExpirationDate();
        var token = SignToken(content, tokenExpiration);

        await EvictSurplusSessionsAsync(userId, ct);
        context.ApiTokens.Add(new ApiToken(userId, ApiToken.Hash(token), userAgent, tokenExpiration));
        await context.SaveChangesAsync(ct);

        return token;
    }

    public async Task<AuthorizationResult> AuthorizeTokenAsync(string? token, CancellationToken ct = default)
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
            var user = await context.Users.FirstOrDefaultAsync(u => u.Id == decoded.Id && u.IsActive, ct);

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

    private string SignToken(TokenContent obj, DateTime expires)
    {
        var claims = new List<Claim>
            { new(AuthenticationStaticOptions.ContentClaimType, JsonSerializer.Serialize(obj)) };
        var parameters = authenticationOptionService.GetTokenParameters();
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = new SigningCredentials(parameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256),
                Issuer = parameters.ValidIssuer,
                Audience = parameters.ValidAudience
            }
        );
        return tokenHandler.WriteToken(token);
    }

    private async Task EvictSurplusSessionsAsync(Guid userId, CancellationToken ct)
    {
        var maxTokenAllowed = AuthenticationStaticOptions.MaxConcurrentSessions;
        var userTokens = await context.ApiTokens
            .Where(t => t.UserId == userId && t.ExpiredAt > DateTime.UtcNow)
            .ToListAsync(ct);

        if (userTokens.Count < maxTokenAllowed)
            return;

        var surplus = userTokens
            .OrderByDescending(t => t.CreatedAt)
            .Skip(maxTokenAllowed - 1)
            .ToList();
        context.ApiTokens.RemoveRange(surplus);
    }
}