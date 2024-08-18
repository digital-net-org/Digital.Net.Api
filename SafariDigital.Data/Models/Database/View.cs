using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("view"), Index(nameof(Title), IsUnique = true)]
public class View : EntityWithId
{
    [Column("title"), Required, MaxLength(1024)]
    public required string Title { get; set; }

    [Column("is_published"), Required]
    public bool IsPublished { get; set; } = false;

    [Column("type"), Required]
    public EViewType Type { get; set; } = EViewType.Page;

    [Column("published_frame_id"), ForeignKey("view_frame")]
    public int? PublishedFrameId { get; set; }

    public virtual ViewFrame? PublishedFrame { get; set; }

    public virtual ICollection<ViewFrame> Frames { get; set; } = [];
}

public enum EViewType
{
    Page,
    Article
}