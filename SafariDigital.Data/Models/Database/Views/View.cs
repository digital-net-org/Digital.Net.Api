using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Database.Frames;

namespace SafariDigital.Data.Models.Database.Views;

[Table("view"), Index(nameof(Title), IsUnique = true)]
public class View : EntityWithGuid
{
    [Column("title"), Required, MaxLength(1024)]
    public required string Title { get; set; }

    [Column("is_published"), Required]
    public bool IsPublished { get; set; } = false;

    [Column("frame_id"), ForeignKey("frame")]
    public Guid? FrameId { get; set; }

    public virtual Frame? Frame { get; set; }
}
