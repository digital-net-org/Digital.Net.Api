using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models;

[Table("Sheet")]
public class Sheet : Entity
{
    [Column("Name")]
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    [Column("Type")]
    [Required]
    [MaxLength(16)]
    public required string Type { get; set; }

    [Column("Content")]
    [Required]
    public required string Content { get; set; }

    [Column("Published")]
    public bool Published { get; set; }

    public virtual List<PageSheet> PageSheets { get; set; } = [];
}
