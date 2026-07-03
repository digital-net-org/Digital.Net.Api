using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Cms.Models;

[Table("Tag")]
public class Tag : Entity
{
    [Column("Name")]
    [Required]
    [MaxLength(128)]
    [Sortable]
    public required string Name { get; set; }

    [Column("Color")]
    [MaxLength(32)]
    [Sortable]
    public string? Color { get; set; }
}
