using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Core.Settings;
using Digital.Net.Api.Entities.Context;
using Digital.Net.Api.Entities.Models.Avatars;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Services;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Documents.Exceptions;
using Digital.Net.Api.Services.Documents.Extensions;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Users;

public class UserService(
    IDocumentService documentService,
    IRepository<User, DigitalContext> userRepository,
    IRepository<Avatar, DigitalContext> avatarRepository) : IUserService
{
    public async Task<Result> UpdatePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var result = new Result();

        if (!PasswordUtils.VerifyPassword(user, currentPassword))
            return result.AddError(new InvalidCredentialsException());
        if (!RegularExpressions.Password.IsMatch(newPassword))
            return result.AddError(new PasswordMalformedException());

        user.Password = PasswordUtils.HashPassword(newPassword);
        userRepository.Update(user);
        await userRepository.SaveAsync();
        return result;
    }

    public async Task<Result<Document>> UpdateAvatar(User user, IFormFile form)
    {
        if (form.Length > ApplicationDefaults.MaxAvatarSize)
            return new Result<Document>().AddError(new TooHeavyException());
        if (!form.IsImage())
            return new Result<Document>().AddError(new UnsupportedFormatException());

        var result = await documentService.SaveImageDocumentAsync(form, user);
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