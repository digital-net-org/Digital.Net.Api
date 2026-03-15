using System.Collections.Generic;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models;
using Digital.Net.Lib.Random;

namespace Digital.Net.Tests.Core.Factories.Data;

public static class TestArticleDataFactory
{
    public static Tag BuildTestTag(this CmsContext context, string? name = null)
    {
        var tag = new Tag { Name = name ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8) };
        context.Tags.Add(tag);
        context.SaveChanges();
        return tag;
    }

    public static Article BuildTestArticle(this CmsContext context, string? path = null, bool published = false, List<Tag>? tags = null)
    {
        var article = new Article
        {
            Path = path ?? $"/{Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 10)}",
            Published = published,
            Name = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8),
            Content = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 16),
            Tags = tags ?? []
        };
        context.Articles.Add(article);
        context.SaveChanges();
        return article;
    }
}
