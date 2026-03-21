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
    public DbSet<Sheet> Sheets { get; init; }
    public DbSet<PageSheet> PageSheets { get; init; }
    public DbSet<Media> Media { get; init; }
    public DbSet<MediaVariant> MediaVariants { get; init; }
    public DbSet<Form> Forms { get; init; }
    public DbSet<FormField> FormFields { get; init; }
    public DbSet<FormSubmission> FormSubmissions { get; init; }

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

        builder.Entity<PageSheet>()
            .HasKey(ps => new { ps.PageId, ps.SheetId });

        builder.Entity<PageSheet>()
            .HasOne(ps => ps.Page)
            .WithMany()
            .HasForeignKey(ps => ps.PageId);

        builder.Entity<PageSheet>()
            .HasOne(ps => ps.Sheet)
            .WithMany(s => s.PageSheets)
            .HasForeignKey(ps => ps.SheetId);

        builder.Entity<MediaVariant>()
            .HasOne(v => v.Media)
            .WithMany(m => m.Variants)
            .HasForeignKey(v => v.MediaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FormField>()
            .HasOne(f => f.Form)
            .WithMany(fm => fm.Fields)
            .HasForeignKey(f => f.FormId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<FormSubmission>()
            .HasOne(s => s.Form)
            .WithMany(fm => fm.Submissions)
            .HasForeignKey(s => s.FormId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
