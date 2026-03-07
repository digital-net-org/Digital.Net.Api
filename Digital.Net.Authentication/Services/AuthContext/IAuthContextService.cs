namespace Digital.Net.Authentication.Services.AuthContext;

public interface IAuthContextService
{
    string? BearerToken { get; }
}