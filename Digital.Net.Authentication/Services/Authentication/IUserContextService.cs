
using Digital.Net.Entities.Models.Users;

namespace Digital.Net.Authentication.Services.Authentication;

public interface IUserContextService
{
    /// <summary>Get the authenticated user id from the HttpContext.</summary>
    public Guid GetUserId();

    /// <summary>Get the authenticated user object from the HttpContext.</summary>
    public User GetUser();
}