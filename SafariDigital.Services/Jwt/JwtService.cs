using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using SafariDigital.Core.Application;
using SafariDigital.Services.Jwt.Models;

namespace SafariDigital.Services.Jwt;

public class JwtService(
    IConfiguration configuration
) : IJwtService
{
    private readonly TokenValidationParameters _tokenParameters = configuration.GetTokenParameters();
    public string GetCookieName() => configuration.GetSettingOrThrow<string>(EApplicationSetting.JwtCookieName);

    public long GetBearerTokenExpiration() =>
        configuration.GetSettingOrThrow<long>(EApplicationSetting.JwtBearerExpiration);

    public long GetRefreshTokenExpiration() =>
        configuration.GetSettingOrThrow<long>(EApplicationSetting.JwtRefreshExpiration);

    public TokenValidationParameters GetTokenParameters() => _tokenParameters;

    public SigningCredentials GetSigningSecret() =>
        new(_tokenParameters.IssuerSigningKey, SecurityAlgorithms.HmacSha256);

    public string GenerateBearerToken<T>(T content) =>
        SignToken(content, DateTime.UtcNow.AddMilliseconds(GetBearerTokenExpiration()));

    public string GenerateRefreshToken<T>(T content) =>
        SignToken(content, DateTime.UtcNow.AddMilliseconds(GetRefreshTokenExpiration()));

    public JwtToken<T> ValidateToken<T>(string? token)
    {
        var result = new JwtToken<T> { Token = token };
        var handler = new JwtSecurityTokenHandler();

        try
        {
            handler.ValidateToken(token, GetTokenParameters(), out var validatedToken);
            var content = handler
                .ReadJwtToken(token)
                .Claims.First(c => c.Type == JwtUtils.ContentClaimType)
                .Value;
            result.Content = JsonSerializer.Deserialize<T>(content);
            result.SecurityToken = validatedToken;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    private string SignToken<T>(T obj, DateTime expires)
    {
        var claims = new List<Claim> { new(JwtUtils.ContentClaimType, JsonSerializer.Serialize(obj)) };
        var parameters = GetTokenParameters();
        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(
            new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expires,
                SigningCredentials = GetSigningSecret(),
                Issuer = parameters.ValidIssuer,
                Audience = parameters.ValidAudience
            }
        );
        return tokenHandler.WriteToken(token);
    }
}