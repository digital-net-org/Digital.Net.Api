using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;

namespace Digital.Net.Cms.Models.Articles;

[Table("Article")]
public class Article : Entity
{
    [Column("Title")]
    [Required]
    [MaxLength(256)]
    public required string Title { get; set; }

    [Column("Description")]
    [Required]
    [MaxLength(512)]
    public required string Description { get; set; }

    [Column("Content")]
    [Required]
    public required string Content { get; set; }

    [Column("Slug")]
    [Required]
    [MaxLength(256)]
    [RegexValidation(RegularExpressions.ArticleSlugPattern)]
    public required string Slug { get; set; }

    [Column("PublishedAt")]
    public DateTime? PublishedAt { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
