using Digital.Lib.Net.Authentication.Services.Security;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Repositories;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Digital.Pages.Core.Application;
using Digital.Pages.Core.Files;
using Digital.Pages.Data.Models.Avatars;
using Digital.Pages.Data.Models.Documents;
using Digital.Pages.Data.Models.Users;
using Digital.Pages.Services.Documents;

namespace Digital.Pages.Services.Users;

public class UserService(
    IConfiguration configuration,
    IDocumentService documentService,
    IHashService hashService,
    IRepository<User> userRepository,
    IRepository<Avatar> avatarRepository) : IUserService
{
    private long MaxAvatarSize => configuration.GetSection<long>(ApplicationSettingPath.FileSystemMaxAvatarSize);

    public async Task<Result> UpdatePassword(User user, string currentPassword, string newPassword)
    {
        var result = new Result();

        if (!HashService.VerifyPassword(user, currentPassword))
            return result.AddError(ApplicationMessageCode.Incorrect);
        if (!hashService.VerifyPasswordValidity(newPassword))
            return result.AddError(ApplicationMessageCode.DoesNotMeetRequirements);

        user.Password = hashService.HashPassword(newPassword);
        userRepository.Update(user);
        await userRepository.SaveAsync();
        return result;
    }

    public async Task<Result<Document>> UpdateAvatar(User user, IFormFile form)
    {
        if (form.Length > MaxAvatarSize)
            return new Result<Document>().AddError(ApplicationMessageCode.TooHeavy);
        if (!form.IsImage())
            return new Result<Document>().AddError(ApplicationMessageCode.InvalidFormat);

        var result = await documentService.SaveImageDocumentAsync(form);
        if (result.HasError() || result.Value is null)
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