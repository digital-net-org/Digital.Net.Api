using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.Pages;

[
    Table("Page"),
    Index(nameof(Path), IsUnique = true)
]
public class Page : EntityGuid
{
    [Column("Title"), Required, MaxLength(64)]
    public required string Title { get; set; }
    
    [Column("Description"), MaxLength(256)]
    public required string Description { get; set; } = string.Empty;

    [Column("Path"), Required, MaxLength(2068)]
    public required string Path { get; set; }

    [Column("IsPublished"), Required]
    public bool IsPublished { get; set; } = false;
    
    [Column("IsIndexed"), Required]
    public bool IsIndexed { get; set; } = true;

    [Column("PuckData"), DataFlag("json")]
    public string? PuckData { get; set; }

    public virtual List<PageMeta> Metas { get; set; } = [];
}