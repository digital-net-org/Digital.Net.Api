using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Cms.Models;

[Table("Tag")]
public class Tag : Entity
{
    [Column("Name")]
    [Required]
    [MaxLength(128)]
    public required string Name { get; set; }

    [Column("Color")]
    [MaxLength(32)]
    public string? Color { get; set; }
}
