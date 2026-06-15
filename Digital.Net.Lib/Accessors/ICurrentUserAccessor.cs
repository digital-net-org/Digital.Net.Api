namespace Digital.Net.Lib.Accessors;

/// <summary>
///     Exposes the current user's identity to the framework.
/// </summary>
public interface ICurrentUserAccessor
{
    /// <summary>Get the current user ID. Throws <see cref="UnauthorizedAccessException" /> if not authenticated.</summary>
    Guid GetUserId();

    /// <summary>Try to get the current user ID. Returns <c>null</c> if no user is found.</summary>
    Guid? TryGetUserId();
}