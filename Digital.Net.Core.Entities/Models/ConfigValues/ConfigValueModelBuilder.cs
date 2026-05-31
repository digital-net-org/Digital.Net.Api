using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Models.ConfigValues;

public static class ConfigValueModelBuilder
{
    public static ModelBuilder BuildConfigValue(this ModelBuilder builder)
    {
        builder.Entity<ConfigValue>()
            .Property(c => c.Type)
            .HasConversion(
                v => v.ToString().ToUpperInvariant(),
                v => Enum.Parse<ConfigValueType>(v, true)
            )
            .HasMaxLength(16);

        return builder;
    }
}