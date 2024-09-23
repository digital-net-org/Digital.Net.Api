using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("view_frame"), Index(nameof(Name), IsUnique = true)]
public class ViewFrame : EntityWithId
{
    [Column("name"), Required, MaxLength(1024)]
    public required string Name { get; set; }

    [Column("view_id"), Required, ForeignKey("view")]
    public int ViewId { get; set; }

    public virtual View View { get; set; } = default!;

    [Column("data")]
    public string? Data { get; set; }
}