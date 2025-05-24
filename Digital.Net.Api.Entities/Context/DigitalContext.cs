using Digital.Net.Api.Entities.Models.ApiKeys;
using Digital.Net.Api.Entities.Models.ApiTokens;
using Digital.Net.Api.Entities.Models.ApplicationOptions;
using Digital.Net.Api.Entities.Models.Avatars;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Events;
using Digital.Net.Api.Entities.Models.Pages;
using Digital.Net.Api.Entities.Models.PuckConfigs;
using Digital.Net.Api.Entities.Models.Users;
using Digital.Net.Api.Entities.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Context;

public class DigitalContext(DbContextOptions<DigitalContext> options) : DbContext(options)
{
    public const string Schema = "digital_net";

    public DbSet<ApiKey> ApiKeys { get; init; }
    public DbSet<ApiToken> ApiTokens { get; init; }
    public DbSet<ApplicationOption> ApplicationOptions { get; init; }
    public DbSet<Avatar> Avatars { get; init; }
    public DbSet<Document> Documents { get; init; }
    public DbSet<Event> Events { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<View> Views { get; init; }
    public DbSet<Page> Pages { get; init; }
    public DbSet<PuckConfig> PuckConfigs { get; init; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);

        builder
            .Entity<User>()
            .HasMany<ApiKey>()
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
            .HasMany<Event>()
            .WithOne(e => e.User)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .Entity<User>()
            .HasOne(u => u.Avatar)
            .WithMany()
            .HasForeignKey(u => u.AvatarId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
