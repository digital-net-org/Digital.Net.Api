using Digital.Net.Cms.Models.Articles;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Cms.Http.Dto;

public class ArticlePublicQuery : Query
{
    public static readonly int MaxNameLength =
        SchemaProperty<Article>.Get().First(p => p.Name == nameof(Article.Title)).MaxLength
        ?? throw new InvalidOperationException($"{nameof(Article)}.{nameof(Article.Title)} must declare a [MaxLength].");

    public string? Name { get; set; }
    public Guid? PageId { get; set; }

    public override void ValidateParameters()
    {
        base.ValidateParameters();
        if (Name is not null && Name.Length > MaxNameLength)
            Name = Name[..MaxNameLength];
    }
}