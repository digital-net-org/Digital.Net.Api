using Safari.Net.Core.Messages;

namespace SafariDigital.Services.Users;

public interface IUserService
{
    Task<Result> UpdatePassword(Data.Models.Database.User user, string currentPassword, string newPassword);
    Task<Result> UpdateAvatar(string id, string avatar);
}