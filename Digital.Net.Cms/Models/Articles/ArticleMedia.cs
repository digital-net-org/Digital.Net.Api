using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Cms.Models.Articles;

[Table("ArticleMedia")]
[PivotResolution("/media", Ownership.Dissociate)]
[Index(nameof(ParentId), nameof(Label), IsUnique = true)]
public class ArticleMedia : Pivot<Article, Media>
{
    [Column("Label")]
    [Required]
    [MaxLength(64)]
    [RegexValidation(RegularExpressions.MediaLabelPattern)]
    public required string Label { get; set; }
}