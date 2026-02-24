using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Api.Entities.Models.Pages;

[Table("PageOpenGraph")]
public class PageOpenGraph : Entity
{
    [Column("Property")]
    [Required]
    [MaxLength(256)]
    public required string Property { get; set; }

    [Column("Content")]
    [Required]
    [MaxLength(2048)]
    public required string Content { get; set; }

    [Column("PageId")]
    [ForeignKey("Page")]
    [Required]
    public Guid PageId { get; set; }

    public virtual Page Page { get; set; }
}