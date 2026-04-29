using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Articles;

public static class ArticleModelBuilder
{
    public static ModelBuilder BuildArticle(this ModelBuilder builder)
    {
        builder.Entity<Article>().ToTable("Article");
        builder
            .Entity<Article>()
            .HasMany(a => a.Tags)
            .WithMany(t => t.Articles)
            .UsingEntity("ArticleTag");
        
        return builder;
    }
}