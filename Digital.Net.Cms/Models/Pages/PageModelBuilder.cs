using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Pages;

public static class PageModelBuilder
{
    public static ModelBuilder BuildPage(this ModelBuilder builder)
    {
        builder.Entity<Page>()
            .Property(p => p.EntityType)
            .HasConversion(
                v => v!.Value.ToString().ToUpperInvariant(),
                v => Enum.Parse<PageEntityType>(v, ignoreCase: true)
            )
            .HasMaxLength(16);
        
        return builder;
    }
}