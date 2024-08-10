using Microsoft.Extensions.Configuration;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Core.Application;
using SafariDigital.Data.Models.Database;
using SafariDigital.Services.Authentication;

namespace SafariDigital.Services.Users;

public class UserService(
    IConfiguration configuration,
    IRepository<User> userRepository,
    IRepository<Avatar> avatarRepository) : IUserService
{
    public async Task<Result> UpdatePassword(User user, string currentPassword, string newPassword)
    {
        var result = new Result();
        var pattern = configuration.GetPasswordRegex();
        if (!AuthenticationUtils.VerifyPassword(user, currentPassword))
            return result.AddError(EApplicationMessage.WrongCredentials);
        if (!AuthenticationUtils.VerifyPasswordValidity(newPassword, pattern))
            return result.AddError(EApplicationMessage.PasswordDoesNotMeetRequirements);

        user.Password = AuthenticationUtils.HashPassword(newPassword);
        userRepository.Update(user);
        await userRepository.SaveAsync();
        return result;
    }

    public async Task<Result> UpdateAvatar(string id, string avatar) => throw new NotImplementedException();
}