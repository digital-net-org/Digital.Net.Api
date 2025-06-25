using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Api.Entities.Models;

/// <summary>
///     Base class for all entities with Timestamps
/// </summary>
public abstract class Entity : EntityMeta, IEntity
{
    [Column("Id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }
}
