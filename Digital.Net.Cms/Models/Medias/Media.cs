using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models.Medias;

[Table("Media")]
public class Media : Entity
{
    [Column("Name")]
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    [Column("Alt")]
    [MaxLength(512)]
    public string? Alt { get; set; }

    [Column("Published")]
    public bool Published { get; set; }

    [Column("DocumentId")]
    [Required]
    [ReadOnly]
    public Guid DocumentId { get; set; }

    [ReadOnly]
    public virtual List<MediaVariant> Variants { get; set; } = [];
}
