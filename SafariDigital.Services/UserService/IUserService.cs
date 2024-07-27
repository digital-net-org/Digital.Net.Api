using SafariDigital.Core.Validation;
using SafariDigital.Database.Models.UserTable;

namespace SafariDigital.Services.UserService;

public interface IUserService
{
    Task<Result> UpdatePassword(User user, string currentPassword, string newPassword);
    Task<Result> UpdateAvatar(string id, string avatar);
}