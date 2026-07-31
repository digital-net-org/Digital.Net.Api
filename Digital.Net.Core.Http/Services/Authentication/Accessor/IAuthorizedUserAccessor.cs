using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Http.Services.Authentication.Types;
using Digital.Net.Lib.Accessors;

namespace Digital.Net.Core.Http.Services.Authentication.Accessor;

/// <summary>
///     The authenticated view of the current user: everything <see cref="IUserAccessor" /> exposes, plus what
///     only the authentication module can answer — the entity itself and the scheme that granted access.
/// </summary>
public interface IAuthorizedUserAccessor : IUserAccessor
{
    /// <summary>
    ///     Get the current user entity. Throws <see cref="UnauthorizedAccessException" /> if not found or not authenticated.
    /// </summary>
    Task<User> GetUserAsync(CancellationToken ct = default);

    /// <summary>
    ///     Get the authorization outcome of the current request, scheme included.
    ///     Throws <see cref="UnauthorizedAccessException" /> when the request went through no authentication filter.
    /// </summary>
    AuthorizationResult GetAuthorizationResult();

    /// <summary>
    ///     Set the authorization outcome of the current request.
    /// </summary>
    void SetAuthorizationResult(AuthorizationResult result);
}