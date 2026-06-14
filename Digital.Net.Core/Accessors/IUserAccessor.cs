using Digital.Net.Core.Entities.Models.Users;

namespace Digital.Net.Core.Accessors;

/// <summary>
///     Expose the request user <see cref="User"/>.
/// </summary>
public interface IUserAccessor
{
    /// <summary>
    ///     Get the current user ID. Throws <see cref="UnauthorizedAccessException" /> if not authenticated.
    /// </summary>
    Guid GetUserId();

    /// <summary>
    ///     Try to get the current user ID. Returns <c>null</c> if no user is found.
    /// </summary>
    Guid? TryGetUserId();

    /// <summary>
    ///     Get the current user entity. Throws <see cref="UnauthorizedAccessException" /> if not found or not authenticated.
    /// </summary>
    Task<User> GetUserAsync(CancellationToken ct = default);
}
