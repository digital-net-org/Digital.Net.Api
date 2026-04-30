using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Avatars;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Entities.Models.Events;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Core.Services.Auditing;
using Digital.Net.Core.Services.Authentication.Exceptions;
using Digital.Net.Core.Services.Authentication.Utils;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Services.Documents.Extensions;
using Digital.Net.Core.Services.Users.Events;
using Digital.Net.Core.Services.Users.Exceptions;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Settings;
using Digital.Net.Lib.String;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.Users;

public class UserService(
    IDocumentService documentService,
    IAuditService auditService,
    DigitalContext context
)
{
    public async Task<Result> UpdatePasswordAsync(User user, string currentPassword, string newPassword)
    {
        var result = new Result();
        Func<EventState, Task> registerEvent = async (state) =>
        {
            await auditService.RegisterEventAsync(
                UserEvents.UpdatePassword,
                state,
                result,
                user.Id
            );
        };

        if (!PasswordUtils.VerifyPassword(user, currentPassword))
        {
            result.AddError(new InvalidCredentialsException());
            await registerEvent(EventState.Failed);
            return result;
        }

        if (!RegularExpressions.Password.IsMatch(newPassword))
        {
            result.AddError(new PasswordMalformedException());
            await registerEvent(EventState.Failed);
            return result;
        }

        user.Password = PasswordUtils.HashPassword(newPassword);
        context.Users.Update(user);
        await context.SaveChangesAsync();
        await registerEvent(EventState.Success);
        return result;
    }

    public async Task<Result> UpdateAvatar(User user, IFormFile form)
    {
        var result = new Result();
        if (form.Length > AppSettings.DefaultMaxAvatarSize)
            return result.AddError(new TooHeavyException());
        if (!form.IsImage())
            return result.AddError(new UnsupportedFormatException());

        var documentResult = await documentService.SaveImageDocumentAsync(form, user);
        result.Merge(documentResult);
        if (result.HasError || documentResult.Value is null)
            return result;
        if (user.AvatarId is not null)
            await RemoveUserAvatar(user);

        return result.Merge(await SaveAvatarAsync(documentResult, user));
    }

    public async Task<Result> RemoveUserAvatar(User user)
    {
        if (user.AvatarId is null)
            return new Result();

        var avatar = await context.Avatars.FindAsync(user.AvatarId);
        if (avatar is null)
            return new Result();

        var documentId = avatar.DocumentId;
        user.AvatarId = null;
        user.Avatar = null;
        context.Users.Update(user);
        context.Avatars.Remove(avatar);
        await context.SaveChangesAsync();
        return await documentService.RemoveDocumentAsync(documentId);
    }

    public async Task<Result<Document>> GetUserAvatarDocumentAsync(Guid userId)
    {
        var result = new Result<Document>();
        var user = await context.Users
            .Include(u => u.Avatar)
            .ThenInclude(a => a!.Document)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user?.Avatar?.Document is not { } document)
            return result.AddError(new DocumentNotFoundException());

        result.Value = document;
        return result;
    }

    public async Task<Result<Guid>> CreateUserAsync(string username, string login, string email, string password)
    {
        var result = new Result<Guid>();

        if (!RegularExpressions.Password.IsMatch(password))
            return result.AddError(new PasswordMalformedException());

        var user = new User
        {
            Username = username,
            Login = login,
            Email = email,
            Password = PasswordUtils.HashPassword(password),
            IsActive = false,
            IsAdmin = false
        };

        await context.Users.AddAsync(user);
        await context.SaveChangesAsync();
        result.Value = user.Id;
        return result;
    }

    public async Task<Result> DeleteUserAsync(Guid userId)
    {
        var result = new Result();
        var user = await context.Users.FindAsync(userId);

        if (user is null)
            return result.AddError(new ResourceNotFoundException());

        if (user.IsAdmin)
            return result.AddError(new CannotDeleteAdminException());

        await RemoveUserAvatar(user);
        context.Users.Remove(user);
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<Result> UpdateUserStatusAsync(Guid userId, bool isActive)
    {
        var result = new Result();
        var user = await context.Users.FindAsync(userId);

        if (user is null)
            return result.AddError(new ResourceNotFoundException());

        if (user.IsAdmin && !isActive)
            return result.AddError(new CannotRevokeAdminException());

        user.IsActive = isActive;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return result;
    }

    public async Task<Result> UpdateUserRoleAsync(Guid userId, bool isAdmin)
    {
        var result = new Result();
        var user = await context.Users.FindAsync(userId);

        if (user is null)
            return result.AddError(new ResourceNotFoundException());

        if (user.IsAdmin && !isAdmin)
            return result.AddError(new CannotDemoteAdminException());

        user.IsAdmin = isAdmin;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return result;
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