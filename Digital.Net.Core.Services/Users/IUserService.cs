using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Core.Services.Users;

public interface IUserService
{
    Task<Result> UpdatePasswordAsync(User user, string currentPassword, string newPassword);
    Task<Result<Document>> UpdateAvatar(User user, IFormFile form);
    Task<Result> RemoveUserAvatar(User user);
}