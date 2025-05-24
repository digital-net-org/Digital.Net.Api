using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Api.Entities.Models;

/// <summary>
///     Base class for entities with a Guid primary key
/// </summary>
public abstract class EntityGuid : Entity
{
    [Column("Id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; init; }
}