using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Articles;

public static class ArticleModelBuilder
{
    public static ModelBuilder BuildArticle(this ModelBuilder builder)
    {
        builder.Entity<Article>()
            .HasOne(a => a.Page)
            .WithMany()
            .HasForeignKey(a => a.PageId)
            .OnDelete(DeleteBehavior.SetNull);

        return builder;
    }
}
