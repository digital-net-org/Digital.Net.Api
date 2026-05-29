using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models.Articles;

[Table("ArticleTag")]
[PivotResolution("/tags", Ownership.Dissociate)]
public class ArticleTag : Pivot<Article, Tag>;