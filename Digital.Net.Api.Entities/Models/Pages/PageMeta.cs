using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Digital.Net.Api.Entities.Models.Pages;

[Table("PageMeta")]
public class PageMeta : Entity
{
    [Column("Key"), MaxLength(128)]
    public required string Key { get; set; }

    [Column("Value"), MaxLength(128)]
    public required string Value { get; set; }
    
    [Column("Content"), Required, MaxLength(256)]
    public required string Content { get; set; }
    
    [Column("PageId"), ForeignKey("Page"), Required]
    public Guid PageId { get; set; }

    public virtual Page Page { get; set; }
}