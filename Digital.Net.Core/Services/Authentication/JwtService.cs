using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Options;
using Digital.Net.Core.Services.Authentication.Types;
using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.ApiTokens;
using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Core.Services.Authentication;

public class JwtService(
    IAuthenticationOptionService authenticationOptionService,
    DigitalContext context
) : IJwtService
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

    public AuthorizationResult AuthorizeToken(string? token)
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