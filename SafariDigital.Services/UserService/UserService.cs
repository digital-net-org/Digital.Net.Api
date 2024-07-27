using Microsoft.Extensions.Configuration;
using SafariDigital.Core.Application;
using SafariDigital.Core.Validation;
using SafariDigital.Database.Models.AvatarTable;
using SafariDigital.Database.Models.UserTable;
using SafariDigital.Database.Repository;
using SafariDigital.Services.AuthenticationService;

namespace SafariDigital.Services.UserService;

public class UserService(
    IConfiguration configuration,
    IRepositoryService<User> userRepository,
    IRepositoryService<Avatar> avatarRepository) : IUserService
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