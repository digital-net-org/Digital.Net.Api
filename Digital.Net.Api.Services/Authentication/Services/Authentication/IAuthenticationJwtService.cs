namespace Digital.Net.Api.Services.Authentication.Services.Authentication;

public interface IAuthenticationJwtService
{
    public Task RevokeTokenAsync(string token);
    public Task RevokeAllTokensAsync(Guid userId);
    public string GenerateBearerToken(Guid userId, string userAgent);
    public string GenerateRefreshToken(Guid userId, string userAgent);
}