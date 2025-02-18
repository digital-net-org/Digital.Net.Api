using Digital.Lib.Net.Authentication.Services.Authentication;
using Digital.Lib.Net.Core.Messages;
using Digital.Lib.Net.Entities.Context;
using Digital.Lib.Net.Entities.Models.Avatars;
using Digital.Lib.Net.Entities.Models.Documents;
using Digital.Lib.Net.Entities.Models.Users;
using Digital.Lib.Net.Entities.Repositories;
using Digital.Lib.Net.Files.Exceptions;
using Digital.Lib.Net.Files.Extensions;
using Digital.Lib.Net.Files.Options;
using Digital.Lib.Net.Files.Services;
using Microsoft.Extensions.Options;

namespace Digital.Core.Api.Services.Users;

public class UserService(
    IOptions<DigitalFilesOptions> filesOptions,
    IDocumentService documentService,
    IAuthenticationService authenticationService,
    IRepository<User, DigitalContext> userRepository,
    IRepository<Avatar, DigitalContext> avatarRepository) : IUserService
{
    public async Task<Result> UpdatePassword(User user, string currentPassword, string newPassword) =>
        await authenticationService.UpdatePasswordAsync(user, currentPassword, newPassword);

    public async Task<Result<Document>> UpdateAvatar(User user, IFormFile form)
    {
        if (form.Length > filesOptions.Value.MaxAvatarSize)
            return new Result<Document>().AddError(new TooHeavyException());
        if (!form.IsImage())
            return new Result<Document>().AddError(new UnsupportedFormatException());

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