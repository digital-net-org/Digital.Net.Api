using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;

namespace Digital.Net.Core.Entities.Models;

public abstract class EntityMeta
{
    [Column("CreatedAt"), Required, ReadOnly]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt"), ReadOnly]
    public DateTime? UpdatedAt { get; set; }
}