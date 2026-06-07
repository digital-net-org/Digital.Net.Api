using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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
public class EntityMutation : Entity, IUntrackedEntity
{
    [Column("ChangeType")]
    [Required]
    public ChangeType ChangeType { get; init; }

    /// <summary>CLR type name of the mutated entity (e.g. <c>"Page"</c>, <c>"User"</c>).</summary>
    [Column("EntityType")]
    [Required]
    [MaxLength(256)]
    public string EntityType { get; init; } = string.Empty;

    /// <summary>Primary key of the mutated entity.</summary>
    [Column("EntityId")]
    [Required]
    public Guid EntityId { get; init; }

    /// <summary>
    ///     Author of the mutation when known (via <c>IUserAccessor</c>); <c>null</c> outside an authenticated HTTP
    ///     context.
    /// </summary>
    [Column("UserId")]
    public Guid? UserId { get; init; }

    [Column("IpAddress")]
    [MaxLength(45)]
    public string? IpAddress { get; init; }

    [Column("UserAgent")]
    [MaxLength(1024)]
    public string? UserAgent { get; init; }
}