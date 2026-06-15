using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Cms.Models.Articles;

[Table("ArticleRelated")]
[PivotResolution("/related", Ownership.Dissociate)]
public class ArticleRelated : Pivot<Article, Article>;