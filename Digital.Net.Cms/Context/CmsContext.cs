using Digital.Net.Cms.Models;
using Digital.Net.Core.Entities.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Context;

public class CmsContext(DbContextOptions<CmsContext> options) : DbContext(options)
{
    public const string Schema = "digital_net_cms";

    public DbSet<Page> Pages { get; init; }
    public DbSet<Article> Articles { get; init; }
    public DbSet<Tag> Tags { get; init; }
    public DbSet<Media> Media { get; init; }
    public DbSet<MediaVariant> MediaVariants { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(
            new TimestampInterceptor()
        );

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);

        builder.Entity<Article>().ToTable("Article");

        builder
            .Entity<Article>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.Articles)
            .UsingEntity("ArticleTag");

        builder.Entity<MediaVariant>()
            .HasOne(v => v.Media)
            .WithMany(m => m.Variants)
            .HasForeignKey(v => v.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
