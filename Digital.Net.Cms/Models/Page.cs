using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Attributes;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models;

[Table("Page")]
[Index(nameof(Path), IsUnique = true)]
public class Page : Entity
{
    [Column("Path")]
    [Required]
    [MaxLength(2068)]
    public required string Path { get; set; }

    [Column("Published")]
    public bool Published { get; set; }

    [Column("Indexed")]
    public bool Indexed { get; set; } = true;

    [Column("Title")]
    [MaxLength(256)]
    public string? Title { get; set; }

    [Column("Description")]
    [MaxLength(512)]
    public string? Description { get; set; }

    [Column("JsonLd")]
    [DataFlag("json")]
    [MaxLength(65535)]
    public string? JsonLd { get; set; }

    [Column("OpenGraph")]
    [DataFlag("json")]
    [MaxLength(65535)]
    public string? OpenGraph { get; set; }

    [Column("Redirect")]
    [MaxLength(2068)]
    public string? Redirect { get; set; }
}
