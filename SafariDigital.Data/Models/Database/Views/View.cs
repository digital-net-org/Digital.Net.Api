using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;
using SafariDigital.Data.Models.Database.Frames;

namespace SafariDigital.Data.Models.Database.Views;

[Table("view"), Index(nameof(Title), IsUnique = true)]
public class View : EntityWithId
{
    [Column("title"), Required, MaxLength(1024)]
    public required string Title { get; set; }

    [Column("is_published"), Required]
    public bool IsPublished { get; set; } = false;

    [Column("frame_id"), ForeignKey("frame")]
    public int? FrameId { get; set; }

    public virtual Frame? Frame { get; set; }
}
