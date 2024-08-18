using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Safari.Net.Core.Extensions.HttpUtilities;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.HttpContext;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.Jwt;

public class JwtService(
    IHttpContextAccessor httpContextAccessor,
    IConfiguration configuration,
    IRepository<RecordedToken> tokenRecordsRepository
) : IJwtService
{
    private int MaxTokenAllowed => configuration.GetSection<int>(EApplicationSetting.JwtMaxTokenAllowed);
    private TokenValidationParameters TokenParams => configuration.GetTokenParameters();
    private long BearerTokenExpiration => configuration.GetBearerTokenExpiration();
    private long RefreshTokenExpiration => configuration.GetRefreshTokenExpiration();

    public AuthenticatedUser GetJwtToken() =>
        httpContextAccessor.HttpContext?.GetItem<AuthenticatedUser>(HttpContextService.Token)
        ?? new AuthenticatedUser();

    public JwtToken<AuthenticatedUser> ValidateBearerToken(string? token) => ValidateToken(token);

    public Result<AuthenticatedUser> ValidateRefreshToken(string? token, string userAgent, string ipAddress)
    {
        var result = ValidateToken(token);
        var userId = result.Value?.Id ?? Guid.Empty;

        var record = tokenRecordsRepository
            .Get(t =>
                t.IpAddress == ipAddress
                && t.UserAgent == userAgent
                && t.Token == token
                && t.User.Id == userId
                && t.ExpiredAt > DateTime.UtcNow
            )
            .FirstOrDefault();

        if (record is null || result.Token is null)
            result.AddError(EApplicationMessage.TokenNotKnown);

        return result;
    }

    public async Task RegisterToken(string token, User user, string userAgent, string ipAddress)
    {
        HandleMaxTokenAllowed(user);
        var record = new RecordedToken
        {
            IpAddress = ipAddress,
            UserAgent = userAgent.Length > 1024 ? userAgent[..1024] : userAgent,
            Token = token,
            UserId = user.Id,
            ExpiredAt = DateTime.UtcNow.AddMilliseconds(RefreshTokenExpiration)
        };

        await tokenRecordsRepository.CreateAsync(record);
        await tokenRecordsRepository.SaveAsync();
    }

    public async Task RevokeToken(string token)
    {
        var record = tokenRecordsRepository.Get(t => t.Token == token).FirstOrDefault();
        if (record is null) return;
        record.ExpiredAt = DateTime.UtcNow;
        await tokenRecordsRepository.SaveAsync();
    }

    public async Task RevokeAllTokens(Guid userId)
    {
        var records = tokenRecordsRepository.Get(t => t.User.Id == userId);
        foreach (var record in records) record.ExpiredAt = DateTime.UtcNow;
        await tokenRecordsRepository.SaveAsync();
    }

    public string GenerateBearerToken(User content) =>
        SignToken(new AuthenticatedUser { Id = content.Id, Role = content.Role },
            DateTime.UtcNow.AddMilliseconds(BearerTokenExpiration)
        );

    public string GenerateRefreshToken(User content) =>
        SignToken(new AuthenticatedUser { Id = content.Id, Role = content.Role },
            DateTime.UtcNow.AddMilliseconds(RefreshTokenExpiration));

    private void HandleMaxTokenAllowed(User user)
    {
        var userTokens = tokenRecordsRepository.Get(t => t.User.Id == user.Id && t.ExpiredAt > DateTime.UtcNow);
        if (userTokens.Count() < MaxTokenAllowed) return;

        var tokens = userTokens.OrderByDescending(t => t.CreatedAt).Skip(MaxTokenAllowed);
        foreach (var token in tokens) token.ExpiredAt = DateTime.UtcNow;
    }

    private string SignToken(AuthenticatedUser obj, DateTime expires)
    {
        var claims = new List<Claim> { new(JwtUtils.ContentClaimType, JsonSerializer.Serialize(obj)) };
        var parameters = TokenParams;
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials =
                    new SigningCredentials(configuration.GetTokenParameters().IssuerSigningKey,
                        SecurityAlgorithms.HmacSha256),
                Issuer = parameters.ValidIssuer,
                Audience = parameters.ValidAudience
            }
        );
        return tokenHandler.WriteToken(token);
    }

    private JwtToken<AuthenticatedUser> ValidateToken(string? token)
    {
        var result = new JwtToken<AuthenticatedUser> { Token = token };
        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(token, TokenParams, out var validatedToken);
            var content = handler
                .ReadJwtToken(token)
                .Claims.First(c => c.Type == JwtUtils.ContentClaimType)
                .Value;
            result.Value = JsonSerializer.Deserialize<AuthenticatedUser>(content);
            result.SecurityToken = validatedToken;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }
}