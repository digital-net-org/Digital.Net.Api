using Digital.Net.Api.Services.Authentication.Models;

namespace Digital.Net.Api.Services.Authentication.Services.Authorization;

public interface IAuthorizationService
{
    /// <summary>
    ///     Get the token key string from the request headers.
    /// </summary>
    /// <returns>The token key string.</returns>
    public string? GetRequestKey();

    /// <summary>
    ///     Validate and authenticate the Token.
    /// </summary>
    /// <param name="key">The token key string.</param>
    /// <returns>The result of the validation.</returns>
    public AuthorizationResult AuthorizeUser(string? key);
}