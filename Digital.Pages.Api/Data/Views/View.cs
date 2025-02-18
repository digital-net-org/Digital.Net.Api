using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Entities.Models;
using Digital.Pages.Api.Data.Frames;
using Microsoft.EntityFrameworkCore;

namespace Digital.Pages.Api.Data.Views;

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