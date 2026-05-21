using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Articles;

[Table("Article")]
[Index(nameof(Slug), IsUnique = true)]
public class Article : Entity
{
    [Column("Title")]
    [Required]
    [Templatable]
    [MaxLength(256)]
    public required string Title { get; set; }

    [Column("Description")]
    [Required]
    [Templatable]
    [MaxLength(512)]
    public required string Description { get; set; }

    [Column("Content")]
    [Required]
    public required string Content { get; set; }

    [Column("Slug")]
    [Required]
    [Templatable]
    [MaxLength(256)]
    [RegexValidation(RegularExpressions.ArticleSlugPattern)]
    public required string Slug { get; set; }

    [Column("PublishedAt")]
    public DateTime? PublishedAt { get; set; }

    [Column("PageId")]
    [ForeignKey("Page")]
    public Guid? PageId { get; set; }

    [ReadOnly]
    public virtual Page? Page { get; set; }

    public virtual ICollection<Tag> Tags { get; set; } = new List<Tag>();
}
