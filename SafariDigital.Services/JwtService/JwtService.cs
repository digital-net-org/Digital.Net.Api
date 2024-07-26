using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Services.HttpContextService;
using SafariDigital.Services.JwtService.Models;

namespace SafariDigital.Services.JwtService;

public class JwtService(
    IHttpContextService httpContextService,
    IConfiguration configuration
) : IJwtService
{
    public JwtToken<AuthenticatedUser> GetJwtToken()
    {
        var context = httpContextService.GetContext();
        var result = new JwtToken<AuthenticatedUser>();
        try
        {
            return context.GetTokenFromContext() ?? result;
        }
        catch (Exception e)
        {
            return result.AddError(e);
        }
    }

    public JwtToken<AuthenticatedUser> ValidateToken(string? token)
    {
        var result = new JwtToken<AuthenticatedUser> { Token = token };
        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(token, configuration.GetTokenParameters(), out var validatedToken);
            var content = handler
                .ReadJwtToken(token)
                .Claims.First(c => c.Type == JwtUtils.ContentClaimType)
                .Value;
            result.Content = JsonSerializer.Deserialize<AuthenticatedUser>(content);
            result.SecurityToken = validatedToken;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    public string GenerateBearerToken(User content) =>
        SignToken(new AuthenticatedUser(content),
            DateTime.UtcNow.AddMilliseconds(configuration.GetBearerTokenExpiration()));

    public string GenerateRefreshToken(User content) =>
        SignToken(new AuthenticatedUser(content),
            DateTime.UtcNow.AddMilliseconds(configuration.GetRefreshTokenExpiration()));


    private string SignToken(AuthenticatedUser obj, DateTime expires)
    {
        var claims = new List<Claim> { new(JwtUtils.ContentClaimType, JsonSerializer.Serialize(obj)) };
        var parameters = configuration.GetTokenParameters();
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
}