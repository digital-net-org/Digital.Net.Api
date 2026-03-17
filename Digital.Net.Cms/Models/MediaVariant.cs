using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models;

[Table("MediaVariant")]
[Index(nameof(MediaId), nameof(Width), nameof(Quality), IsUnique = true)]
public class MediaVariant : Entity
{
    [Column("MediaId")]
    [Required]
    public Guid MediaId { get; set; }

    [Column("DocumentId")]
    [Required]
    public Guid DocumentId { get; set; }

    [Column("Width")]
    [Required]
    public int Width { get; set; }

    [Column("Height")]
    [Required]
    public int Height { get; set; }

    [Column("Quality")]
    [Required]
    public int Quality { get; set; }

    public virtual Media Media { get; set; } = null!;
}
