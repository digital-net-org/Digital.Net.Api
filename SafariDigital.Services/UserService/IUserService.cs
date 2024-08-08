using Safari.Net.Core.Messages;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Services.UserService;

public interface IUserService
{
    Task<Result> UpdatePassword(User user, string currentPassword, string newPassword);
    Task<Result> UpdateAvatar(string id, string avatar);
}