using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.Pages;

[
    Table("Page"),
    Index(nameof(Title), IsUnique = true),
    Index(nameof(Path), IsUnique = true)
]
public class Page : EntityGuid
{
    [Column("Title"), Required, MaxLength(1024)]
    public required string Title { get; set; }

    [Column("Path"), Required, MaxLength(128)]
    public required string Path { get; set; }

    [Column("IsPublished"), Required]
    public bool IsPublished { get; set; } = false;

    [Column("ViewId"), ForeignKey("View")]
    public Guid? ViewId { get; set; }

    public virtual View? View { get; set; }
}