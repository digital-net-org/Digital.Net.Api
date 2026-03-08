using Digital.Net.Api.Services.Authentication.Exceptions;
using Digital.Net.Api.Services.Authentication.Utils;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Documents.Exceptions;
using Digital.Net.Api.Services.Documents.Extensions;
using Digital.Net.Core.Messages;
using Digital.Net.Core.Settings;
using Digital.Net.Core.String;
using Digital.Net.Entities.Context;
using Digital.Net.Entities.Models.Avatars;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Users;
using Microsoft.AspNetCore.Http;

namespace Digital.Net.Api.Services.Users;

public class UserService(
    IDocumentService documentService,
    DigitalContext context) : IUserService
{
    public async Task<Result> UpdatePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var result = new Result();

        if (!PasswordUtils.VerifyPassword(user, currentPassword))
            return result.AddError(new InvalidCredentialsException());
        if (!RegularExpressions.Password.IsMatch(newPassword))
            return result.AddError(new PasswordMalformedException());

        user.Password = PasswordUtils.HashPassword(newPassword);
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<Result<Document>> UpdateAvatar(User user, IFormFile form)
    {
        if (form.Length > AppSettings.DefaultMaxAvatarSize)
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
        context.Avatars.Remove(user.Avatar!);
        await context.SaveChangesAsync();
        return await documentService.RemoveDocumentAsync(documentId);
    }

    private async Task<Result<Document>> SaveAvatarAsync(Result<Document> result, User user)
    {
        try
        {
            var avatar = new Avatar { DocumentId = result.Value!.Id };
            await context.Avatars.AddAsync(avatar);
            await context.SaveChangesAsync();

            user.AvatarId = avatar.Id;
            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            result.AddError(ex);
            await documentService.RemoveDocumentAsync(result.Value!.Id);
        }

        return result;
    }
}