using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Safari.Net.Core.Messages;
using Safari.Net.Data.Repositories;
using SafariDigital.Core.Application;
using SafariDigital.Core.Files;
using SafariDigital.Data.Models.Database.Avatars;
using SafariDigital.Data.Models.Database.Documents;
using SafariDigital.Data.Models.Database.Users;
using SafariDigital.Services.Authentication;
using SafariDigital.Services.Documents;

namespace SafariDigital.Services.Users;

public class UserService(
    IConfiguration configuration,
    IDocumentService documentService,
    IRepository<User> userRepository,
    IRepository<Avatar> avatarRepository) : IUserService
{
    private long MaxAvatarSize => configuration.GetSection<long>(EApplicationSetting.FileSystemMaxAvatarSize);

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

    public async Task<Result<Document>> UpdateAvatar(User user, IFormFile form)
    {
        if (form.Length > MaxAvatarSize)
            return new Result<Document>().AddError(EApplicationMessage.AvatarSizeTooHeavy);
        if (!form.IsImage())
            return new Result<Document>().AddError(EApplicationMessage.AvatarInvalidFormat);

        var result = await documentService.SaveImageDocumentAsync(form, EDocumentType.Avatar);
        if (result.HasError || result.Value is null)
            return result;
        if (user.AvatarId is not null)
            await RemoveUserAvatar(user);

        return await SaveAvatarAsync(result, user);
    }

    public async Task<Result> RemoveUserAvatar(User user)
    {
        var documentId = user.Avatar!.DocumentId;
        user.AvatarId = null;
        avatarRepository.Delete(user.Avatar!);
        await userRepository.SaveAsync();
        await avatarRepository.SaveAsync();
        return await documentService.RemoveDocumentAsync(documentId);
    }

    private async Task<Result<Document>> SaveAvatarAsync(Result<Document> result, User user)
    {
        try
        {
            var avatar = new Avatar { DocumentId = result.Value!.Id };
            await avatarRepository.CreateAsync(avatar);
            await avatarRepository.SaveAsync();

            user.AvatarId = avatar.Id;
            await userRepository.SaveAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
            await documentService.RemoveDocumentAsync(result.Value!.Id);
        }

        return result;
    }
}