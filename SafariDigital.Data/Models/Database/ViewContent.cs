using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("view_content")]
public class ViewContent : EntityWithGuid
{
    [Column("type"), Required, MaxLength(128)]
    public required string Type { get; set; }

    [Column("props"), Required]
    public string Props { get; set; } = "{}";

    [Column("view_frame_id"), Required, ForeignKey("view_frame")]
    public int ViewFrameId { get; set; }

    public virtual ViewFrame ViewFrame { get; set; } = default!;
}