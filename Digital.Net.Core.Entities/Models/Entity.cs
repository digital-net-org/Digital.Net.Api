using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Core.Entities.Models;

/// <summary>
///     Base class for all DB entities used in Digital.Net projects.
/// </summary>
public abstract class Entity : EntityMeta
{
    [Column("Id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }
}
