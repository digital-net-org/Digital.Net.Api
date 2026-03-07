using Digital.Net.Entities.Interceptors;
using Digital.Net.Entities.Models.ApiKeys;
using Digital.Net.Entities.Models.ApiTokens;
using Digital.Net.Entities.Models.Avatars;
using Digital.Net.Entities.Models.Documents;
using Digital.Net.Entities.Models.Events;
using Digital.Net.Entities.Models.Pages;
using Digital.Net.Entities.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Entities.Context;

public class DigitalContext(DbContextOptions<DigitalContext> options) : DbContext(options)
{
    public const string Schema = "digital_net";

    public DbSet<ApiKey> ApiKeys { get; init; }
    public DbSet<ApiToken> ApiTokens { get; init; }
    public DbSet<Avatar> Avatars { get; init; }
    public DbSet<Document> Documents { get; init; }
    public DbSet<Event> Events { get; init; }
    public DbSet<User> Users { get; init; }
    public DbSet<Page> Pages { get; init; }
    public DbSet<PageOpenGraph> PageMetas { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) => 
        optionsBuilder.AddInterceptors(
            new TimestampInterceptor()
        );

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
