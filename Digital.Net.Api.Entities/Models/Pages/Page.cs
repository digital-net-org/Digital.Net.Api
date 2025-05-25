using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Attributes;
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

    [Column("PuckData"), DataFlag("json")]
    public string? PuckData { get; set; }

    [Column("PuckConfigId"), Required, ForeignKey("PuckConfig")]
    public required int PuckConfigId { get; set; }
    
    public virtual PagePuckConfig PuckConfig { get; set; }
}