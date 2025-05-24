
using Digital.Net.Api.Entities.Models.Users;

namespace Digital.Net.Api.Services.Authentication.Services.Authentication;

public interface IUserContextService
{
    public Guid GetUserId();
    public User GetUser();
}