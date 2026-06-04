using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Medias;

public static class MediaVariantModelBuilder
{
    public static ModelBuilder BuildMediaVariant(this ModelBuilder builder)
    {
        builder.Entity<MediaVariant>()
            .HasOne(v => v.Media)
            .WithMany(m => m.Variants)
            .HasForeignKey(v => v.MediaId)
            .OnDelete(DeleteBehavior.Cascade);
        return builder;
    }
}