using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Core.Entities.Models;

/// <summary>
///     Base class for all DB entities used in Digital.Net projects.
/// </summary>
public abstract class Entity : EntityMeta, IEntity
{
    [Column("Id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }

    /// <summary>
    ///     Strips EF Core proxy suffix so the dictionary key matches the logical type name (e.g. <c>"Article"</c>,
    ///     not <c>"ArticleProxy"</c>).
    /// </summary>
    public Type GetCanonicalType()
    {
        var type = GetType();
        return type.Name.EndsWith("Proxy") && type.BaseType is not null ? type.BaseType : type;
    }
}
