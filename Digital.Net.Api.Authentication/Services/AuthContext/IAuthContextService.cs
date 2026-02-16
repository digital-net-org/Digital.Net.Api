namespace Digital.Net.Api.Authentication.Services.AuthContext;

public interface IAuthContextService
{
    string? BearerToken { get; }
}