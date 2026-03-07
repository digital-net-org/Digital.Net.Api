using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Digital.Net.Authentication.Options;
using Digital.Net.Authentication.Services.Authorization;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models.ApiTokens;
using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Authentication.Services.Authentication;

public class AuthenticationJwtService(
    IAuthenticationOptionService authenticationOptionService,
    DigitalContext context
) : IAuthenticationJwtService
{
    public async Task RevokeTokenAsync(string token)
    {
        var record = context.ApiTokens.FirstOrDefault(t => t.Key == token);

        if (record is null)
            return;

        context.ApiTokens.Remove(record);
        await context.SaveChangesAsync();
    }

    public async Task RevokeAllTokensAsync(Guid userId)
    {
        var records = context.ApiTokens.Where(t => t.UserId == userId).ToList();

        foreach (var record in records)
        {
            context.ApiTokens.Remove(record);
            await context.SaveChangesAsync();
        }
    }

    public string GenerateBearerToken(Guid userId, string userAgent)
    {
        var content = new TokenContent(userId, userAgent);
        return SignToken(content, authenticationOptionService.GetBearerTokenExpirationDate());
    }

    public string GenerateRefreshToken(Guid userId, string userAgent)
    {
        var content = new TokenContent(userId, userAgent);
        var tokenExpiration = authenticationOptionService.GetRefreshTokenExpirationDate();
        var token = SignToken(content, tokenExpiration);

        HandleMaxConcurrentSessions(userId);
        context.ApiTokens.Add(new ApiToken(userId, token, userAgent, tokenExpiration));
        context.SaveChanges();

        return token;
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

    private void HandleMaxConcurrentSessions(Guid userId)
    {
        var maxTokenAllowed = AuthenticationStaticOptions.MaxConcurrentSessions;
        var userTokens = context.ApiTokens
            .Where(t => t.UserId == userId && t.ExpiredAt > DateTime.UtcNow)
            .ToList();

        if (userTokens.Count < maxTokenAllowed)
            return;

        var tokens = userTokens
            .OrderByDescending(t => t.CreatedAt)
            .Skip(maxTokenAllowed - 1);

        foreach (var token in tokens)
        {
            context.ApiTokens.Remove(token);
            context.SaveChanges();
        }
    }
}