using Digital.Lib.Net.Core.Messages;
using Microsoft.AspNetCore.Http;
using Digital.Pages.Data.Models.Documents;
using Digital.Pages.Data.Models.Users;

namespace Digital.Pages.Services.Users;

public interface IUserService
{
    Task<Result> UpdatePassword(User user, string currentPassword, string newPassword);
    Task<Result<Document>> UpdateAvatar(User user, IFormFile form);
    Task<Result> RemoveUserAvatar(User user);
}