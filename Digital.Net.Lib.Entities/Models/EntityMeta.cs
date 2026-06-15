using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;

namespace Digital.Net.Lib.Entities.Models;

public abstract class EntityMeta
{
    [Column("CreatedAt"), Required, ReadOnly]
    public DateTime CreatedAt { get; set; }

    [Column("UpdatedAt"), ReadOnly]
    public DateTime? UpdatedAt { get; set; }
}