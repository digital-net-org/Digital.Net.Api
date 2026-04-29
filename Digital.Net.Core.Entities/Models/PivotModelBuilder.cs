using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Models;

public static class PivotModelBuilder
{
    /// <summary>
    ///     Scans every entity in the model, detects those deriving from <see cref="Pivot{TParent,TChild}"/>,
    ///     and configures their composite key + parent/child FKs with cascade delete. Call once from
    ///     <c>OnModelCreating</c> after schema defaults so every new pivot is wired without boilerplate.
    /// </summary>
    public static ModelBuilder ConfigurePivots(this ModelBuilder builder)
    {
        foreach (var entity in builder.Model.GetEntityTypes().ToList())
        {
            var baseType = entity.ClrType.BaseType;
            if (baseType is null || !baseType.IsGenericType) continue;
            if (baseType.GetGenericTypeDefinition() != typeof(Pivot<,>)) continue;

            var parentType = baseType.GetGenericArguments()[0];
            var childType = baseType.GetGenericArguments()[1];

            builder.Entity(entity.ClrType)
                .HasKey(nameof(Pivot<Entity, Entity>.ParentId), nameof(Pivot<Entity, Entity>.ChildId));

            builder.Entity(entity.ClrType)
                .HasOne(parentType, nameof(Pivot<Entity, Entity>.Parent))
                .WithMany()
                .HasForeignKey(nameof(Pivot<Entity, Entity>.ParentId))
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity(entity.ClrType)
                .HasOne(childType, nameof(Pivot<Entity, Entity>.Child))
                .WithMany()
                .HasForeignKey(nameof(Pivot<Entity, Entity>.ChildId))
                .OnDelete(DeleteBehavior.Cascade);
        }

        return builder;
    }
}
