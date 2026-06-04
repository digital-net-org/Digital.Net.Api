using Digital.Net.Core.Entities.Models.Users;

namespace Digital.Net.Core.Accessors;

/// <summary>
///     Expose the request user <see cref="User"/>.
/// </summary>
public interface IUserAccessor
{
    Guid GetUserId();

    User GetUser();
}
