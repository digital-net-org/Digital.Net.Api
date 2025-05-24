using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Users;

public interface IUserService
{
    Task<Result> UpdatePasswordAsync(User user, string currentPassword, string newPassword);
    Task<Result<Document>> UpdateAvatar(User user, IFormFile form);
    Task<Result> RemoveUserAvatar(User user);
}