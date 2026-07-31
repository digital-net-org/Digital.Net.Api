namespace Digital.Net.Lib.Accessors;

/// <summary>
///     Exposes the current user's identity to the framework. Part of the base library contract: the host must
///     provide an implementation.
/// </summary>
/// <remarks>
///     Deliberately limited to the identifier so the lower layers can stamp an author without ever seeing
///     the <c>User</c> entity.
/// </remarks>
public interface IUserAccessor
{
    /// <summary>Get the current user ID. Throws <see cref="UnauthorizedAccessException" /> if not authenticated.</summary>
    Guid GetUserId();

    /// <summary>Try to get the current user ID. Returns <c>null</c> if no user is found.</summary>
    Guid? TryGetUserId();
}