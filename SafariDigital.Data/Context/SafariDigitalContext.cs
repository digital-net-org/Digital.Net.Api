using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Context;

public class SafariDigitalContext(DbContextOptions<SafariDigitalContext> options)
    : DbContext(options)
{
    public DbSet<User> Users { get; init; }
    public DbSet<Document> Documents { get; init; }
    public DbSet<Avatar> Avatars { get; init; }
    public DbSet<RecordedLogin> RecordedLogins { get; init; }
    public DbSet<RecordedToken> RecordedTokens { get; init; }
    public DbSet<View> Views { get; init; }
    public DbSet<ViewFrame> ViewFrames { get; init; }
    public DbSet<ViewContent> ViewContents { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<View>()
            .HasMany(v => v.Frames)
            .WithOne(v => v.View)
            .HasForeignKey(v => v.ViewId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<View>()
            .HasOne(v => v.PublishedFrame)
            .WithOne()
            .HasForeignKey<View>(v => v.PublishedFrameId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}