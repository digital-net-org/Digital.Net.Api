using Digital.Net.Lib.Messages;

namespace Digital.Net.Core.Services.Authentication;

public interface IAuthenticationService
{
    public Task<Result<(string bearer, string? refresh)>> RefreshTokensAsync(string? refreshToken, string? userAgent = null);
    public Task<Result<(string bearer, string refresh)>> LoginAsync(
        string login,
        string password,
        string ipAddress,
        string? userAgent = null
    );
    public Task<Result> LogoutAsync(
        string? refreshToken,
        Guid? userId,
        string? userAgent = null,
        string? ipAddress = null
    );
    public Task<Result> LogoutAllAsync(
        string? refreshToken,
        string? userAgent = null,
        string? ipAddress = null
    );
}