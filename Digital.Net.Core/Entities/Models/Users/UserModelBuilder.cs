using Digital.Net.Core.Entities.Models.ApiTokens;
using Digital.Net.Core.Entities.Models.Documents;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Models.Users;

public static class UserModelBuilder
{
    public static ModelBuilder BuildUser(this ModelBuilder builder)
    {
        builder
            .Entity<User>()
            .HasMany(u => u.ApiKeys)
            .WithOne(ak => ak.User)
            .HasForeignKey(ak => ak.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<User>()
            .HasMany<ApiToken>()
            .WithOne(at => at.User)
            .HasForeignKey(at => at.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<User>()
            .HasMany<Document>()
            .WithOne(d => d.Uploader)
            .HasForeignKey(d => d.UploaderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<User>()
            .HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);

        return builder;
    }
}