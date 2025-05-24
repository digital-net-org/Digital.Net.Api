using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Options;
using Digital.Net.Api.Services.Authentication.Services.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Digital.Net.Api.Services.Authentication.Services.Authentication;

public class AuthenticationJwtService(
    IAuthenticationOptionService authenticationOptionService,
    IRepository<ApiToken, DigitalContext> apiTokenRepository
) : IAuthenticationJwtService
{
    public async Task RevokeTokenAsync(string token)
    {
        var record = apiTokenRepository
            .Get(t => t.Key == token)
            .FirstOrDefault();

        if (record is null)
            return;

        apiTokenRepository.Delete(record);
        await apiTokenRepository.SaveAsync();
    }

    public async Task RevokeAllTokensAsync(Guid userId)
    {
        var records = apiTokenRepository
            .Get(t => t.UserId == userId)
            .ToList();

        foreach (var record in records)
        {
            apiTokenRepository.Delete(record);
            await apiTokenRepository.SaveAsync();
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
        apiTokenRepository.Create(new ApiToken(userId, token, userAgent, tokenExpiration));
        apiTokenRepository.Save();

        return token;
    }

    private string SignToken(TokenContent obj, DateTime expires)
    {
        var claims = new List<Claim> { new(DefaultAuthenticationOptions.ContentClaimType, JsonSerializer.Serialize(obj)) };
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
        var maxTokenAllowed = DefaultAuthenticationOptions.MaxConcurrentSessions;
        var userTokens = apiTokenRepository
            .Get(t => t.UserId == userId && t.ExpiredAt > DateTime.UtcNow)
            .ToList();

        if (userTokens.Count < maxTokenAllowed)
            return;

        var tokens = userTokens
            .OrderByDescending(t => t.CreatedAt)
            .Skip(maxTokenAllowed - 1);

        foreach (var token in tokens)
        {
            apiTokenRepository.Delete(token);
            apiTokenRepository.Save();
        }
    }
}