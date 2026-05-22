using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;

namespace Digital.Net.Cms.Models.Articles;

[Table("ArticleMedia")]
[PivotResolution("/media", Ownership.Dissociate)]
public class ArticleMedia : Pivot<Article, Media>
{
    [Column("Label")]
    [MaxLength(64)]
    [RegexValidation(RegularExpressions.MediaLabelPattern)]
    public string? Label { get; set; }
}