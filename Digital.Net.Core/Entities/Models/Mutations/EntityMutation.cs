using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Models.Mutations;

/// <summary>
///     Audit trail of a single mutation on a tracked entity:
///     <list type="bullet">
///         <item>
///             what changed <see cref="ChangeType" />
///         </item>
///         <item>
///             on which entity <see cref="EntityType" /> / <see cref="EntityId" />
///         </item>
///         <item>
///             by whom <see cref="UserId" /> / <see cref="IpAddress" /> / <see cref="UserAgent" />
///         </item>
///         <item>
///             and when <see cref="EntityMeta.CreatedAt" />
///         </item>
///     </list>
///     Materialized per schema.
/// </summary>
[Table("EntityMutation")]
[Index(nameof(CreatedAt), nameof(Id))]
[Index(nameof(EntityType), nameof(EntityId), nameof(CreatedAt))]
public class EntityMutation : Entity, IUntrackedEntity
{
    [Column("ChangeType")]
    [Required]
    public ChangeType ChangeType { get; init; }
    
    [Column("EntityType")]
    [Required]
    [MaxLength(256)]
    public string EntityType { get; init; } = string.Empty;

    [Column("EntityId")]
    [Required]
    public Guid EntityId { get; init; }

    [Column("UserId")]
    public Guid? UserId { get; init; }

    [Column("IpAddress")]
    [MaxLength(45)]
    public string? IpAddress { get; init; }

    [Column("UserAgent")]
    [MaxLength(1024)]
    public string? UserAgent { get; init; }

    [Column("Payload")]
    public string? Payload { get; init; } = string.Empty;
}