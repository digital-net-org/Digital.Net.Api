using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.Entities.Models;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Pages;

[Table("Page")]
[Index(nameof(Path), IsUnique = true)]
public class Page : Entity
{
    [Column("Path")]
    [Required]
    [MaxLength(2068)]
    [RegexValidation(RegularExpressions.PagePathPattern)]
    public required string Path { get; set; }

    [Column("EntityType")]
    [MaxLength(16)]
    public PageEntityType? EntityType { get; set; }

    [Column("Published")]
    public bool Published { get; set; }

    [Column("Indexed")]
    public bool Indexed { get; set; } = true;

    [Column("Title")]
    [Templatable]
    [MaxLength(256)]
    public string? Title { get; set; }

    [Column("Description")]
    [Templatable]
    [MaxLength(512)]
    public string? Description { get; set; }

    [Column("JsonLd")]
    [Templatable]
    [MaxLength(65535)]
    public string? JsonLd { get; set; }

    [Column("Redirect")]
    [MaxLength(2068)]
    public string? Redirect { get; set; }
}
