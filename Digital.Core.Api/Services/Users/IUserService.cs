using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Models.Documents;
using Digital.Lib.Net.Entities.Models.Users;

namespace Digital.Core.Api.Services.Users;

public interface IUserService
{
    Task<Result> UpdatePassword(User user, string currentPassword, string newPassword);
    Task<Result<Document>> UpdateAvatar(User user, IFormFile form);
    Task<Result> RemoveUserAvatar(User user);
}