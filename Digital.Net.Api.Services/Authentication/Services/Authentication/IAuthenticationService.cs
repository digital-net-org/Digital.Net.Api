using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Users;

namespace Digital.Net.Api.Services.Authentication.Services.Authentication;

public interface IAuthenticationService
{
    public Task<Result<User>> ValidateCredentialsAsync(string login, string password);
    public Task<Result<(Guid, string)>> RefreshTokensAsync(string? refreshToken, string? userAgent = null);
    public Task<Result<(Guid, string)>> LoginAsync(
        string login,
        string password,
        string? userAgent = null,
        string? ipAddress = null
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