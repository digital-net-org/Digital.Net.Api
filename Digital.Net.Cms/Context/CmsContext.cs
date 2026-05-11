using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Articles;
using Digital.Net.Cms.Models.Forms;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Core.Entities.Interceptors;
using Digital.Net.Core.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Context;

public class CmsContext(DbContextOptions<CmsContext> options) : DbContext(options)
{
    public const string Schema = "digital_net_cms";

    public DbSet<Page> Pages { get; init; }
    public DbSet<Article> Articles { get; init; }
    public DbSet<ArticleTag> ArticleTags { get; init; }
    public DbSet<Tag> Tags { get; init; }
    public DbSet<Sheet> Sheets { get; init; }
    public DbSet<PageSheet> PageSheets { get; init; }
    public DbSet<OpenGraphEntry> OpenGraphEntries { get; init; }
    public DbSet<PageOpenGraph> PageOpenGraphs { get; init; }
    public DbSet<Media> Media { get; init; }
    public DbSet<MediaVariant> MediaVariants { get; init; }
    public DbSet<Form> Forms { get; init; }
    public DbSet<FormField> FormFields { get; init; }
    public DbSet<FormSubmission> FormSubmissions { get; init; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
        optionsBuilder.AddInterceptors(new TimestampInterceptor());

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.HasDefaultSchema(Schema);
        builder.BuildPage();
        builder.BuildArticle();
        builder.BuildMediaVariant();
        builder.BuildFormField();
        builder.BuildFormSubmission();
        builder.ConfigurePivots();
    }
}
