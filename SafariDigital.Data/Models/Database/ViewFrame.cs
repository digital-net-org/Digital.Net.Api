using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("view_frame"), Index(nameof(Title), IsUnique = true)]
public class ViewFrame : EntityWithId
{
    [Column("title"), Required, MaxLength(1024)]
    public required string Title { get; set; }

    [Column("view_id"), Required, ForeignKey("view")]
    public int ViewId { get; set; }

    public virtual View View { get; set; } = default!;

    public virtual ICollection<ViewContent> Content { get; set; } = [];
}