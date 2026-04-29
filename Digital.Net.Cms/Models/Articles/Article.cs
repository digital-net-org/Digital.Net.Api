using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Cms.Models.Pages;

namespace Digital.Net.Cms.Models.Articles;

[Table("Article")]
public class Article : Page
{
    [Column("Name")]
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    [Column("Content")]
    [Required]
    public required string Content { get; set; }

    public virtual List<Tag> Tags { get; set; } = [];
}
