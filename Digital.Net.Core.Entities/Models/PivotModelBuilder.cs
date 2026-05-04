using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Digital.Net.Core.Entities.Models;

public static class PivotModelBuilder
{
    /// <summary>
    ///     Scans every entity in the model, detects those deriving from <see cref="Pivot{TParent,TChild}"/>,
    ///     and configures their composite key + parent/child FKs with cascade delete. If <typeparamref name="TParent"/>
    ///     (and/or <typeparamref name="TChild"/>) exposes an <c>ICollection&lt;TOther&gt;</c> navigation,
    ///     the pivot is wired as a skip-navigation through <c>UsingEntity</c> so <c>.Include(...)</c> works
    ///     transparently. Call once from <c>OnModelCreating</c> after schema defaults.
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
            var pivotType = entity.ClrType;

            var parentNav = FindCollectionNavigation(parentType, childType);
            var childNav = FindCollectionNavigation(childType, parentType);

            typeof(PivotModelBuilder)
                .GetMethod(nameof(ConfigurePivot), BindingFlags.NonPublic | BindingFlags.Static)!
                .MakeGenericMethod(parentType, childType, pivotType)
                .Invoke(null, [builder, parentNav, childNav]);
        }

        return builder;
    }

    private static string? FindCollectionNavigation(Type owner, Type element) =>
        owner
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(p =>
                p.PropertyType.IsGenericType &&
                p.PropertyType.GetGenericArguments() is [{ } arg] &&
                arg == element &&
                typeof(IEnumerable<>).MakeGenericType(element).IsAssignableFrom(p.PropertyType))
            ?.Name;

    private static void ConfigurePivot<TParent, TChild, TPivot>(
        ModelBuilder builder,
        string? parentNavName,
        string? childNavName
    )
        where TParent : Entity
        where TChild : Entity
        where TPivot : Pivot<TParent, TChild>
    {
        if (parentNavName is null && childNavName is null)
        {
            ConfigureStandalone<TParent, TChild, TPivot>(builder);
            return;
        }

        var hasMany = parentNavName is not null
            ? builder.Entity<TParent>().HasMany<TChild>(parentNavName)
            : builder.Entity<TParent>().HasMany<TChild>();

        var withMany = childNavName is not null
            ? hasMany.WithMany(childNavName)
            : hasMany.WithMany();

        withMany.UsingEntity<TPivot>(
            r => r.HasOne(p => p.Child)
                .WithMany()
                .HasForeignKey(p => p.ChildId)
                .OnDelete(DeleteBehavior.Cascade),
            l => l.HasOne(p => p.Parent)
                .WithMany()
                .HasForeignKey(p => p.ParentId)
                .OnDelete(DeleteBehavior.Cascade),
            j => j.HasKey(p => new { p.ParentId, p.ChildId })
        );
    }

    private static void ConfigureStandalone<TParent, TChild, TPivot>(ModelBuilder builder)
        where TParent : Entity
        where TChild : Entity
        where TPivot : Pivot<TParent, TChild>
    {
        builder.Entity<TPivot>().HasKey(p => new { p.ParentId, p.ChildId });
        builder.Entity<TPivot>()
            .HasOne(p => p.Parent)
            .WithMany()
            .HasForeignKey(p => p.ParentId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Entity<TPivot>()
            .HasOne(p => p.Child)
            .WithMany()
            .HasForeignKey(p => p.ChildId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
