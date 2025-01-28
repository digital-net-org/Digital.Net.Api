using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Frames;

namespace SafariDigital.Data.Models.Views;

[
    Table("View"),
    Index(nameof(Title), IsUnique = true),
    Index(nameof(Path), IsUnique = true)
]
public class View : EntityGuid
{
    [Column("Title"), Required, MaxLength(1024)]
    public required string Title { get; set; }

    [Column("Path"), Required, MaxLength(128)]
    public required string Path { get; set; }

    [Column("IsPublished"), Required]
    public bool IsPublished { get; set; } = false;

    [Column("FrameId"), ForeignKey("Frame")]
    public Guid? FrameId { get; set; }

    public virtual Frame? Frame { get; set; }
}