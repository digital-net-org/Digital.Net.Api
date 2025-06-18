using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Api.Entities.Models.Pages;

[Table("PageMeta")]
public class PageMeta : EntityId
{
    [Column("Name"), MaxLength(128)]
    public string? Name { get; set; }

    [Column("Property"), MaxLength(128)]
    public string? Property { get; set; }
    
    [Column("Content"), Required, MaxLength(256)]
    public required string Content { get; set; }
    
    [Column("PageId"), ForeignKey("Page"), Required]
    public Guid PageId { get; set; }

    public virtual Page Page { get; set; }
}