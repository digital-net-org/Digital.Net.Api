using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Accessors;

namespace Digital.Net.Core.Accessors;

/// <summary>
///     Expose the request user <see cref="User"/>.
/// </summary>
public interface IUserAccessor : ICurrentUserAccessor
{
    /// <summary>
    ///     Get the current user entity. Throws <see cref="UnauthorizedAccessException" /> if not found or not authenticated.
    /// </summary>
    Task<User> GetUserAsync(CancellationToken ct = default);
}
