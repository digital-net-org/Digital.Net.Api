using Digital.Net.Core.Messages;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Users;

public interface IUserService
{
    Task<Result> UpdatePasswordAsync(User user, string currentPassword, string newPassword);
    Task<Result> UpdateAvatar(User user, IFormFile form);
    Task<Result> RemoveUserAvatar(User user);
    Task<Result<Document>> GetUserAvatarDocumentAsync(Guid userId);
    Task<Result<Guid>> CreateUserAsync(string username, string login, string email, string password);
    Task<Result> DeleteUserAsync(Guid userId);
    Task<Result> UpdateUserStatusAsync(Guid userId, bool isActive);
    Task<Result> UpdateUserRoleAsync(Guid userId, bool isAdmin);
}