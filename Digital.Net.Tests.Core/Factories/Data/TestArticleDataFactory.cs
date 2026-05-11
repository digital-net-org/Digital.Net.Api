using System;
using System.Collections.Generic;
using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Articles;
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

    public static Article BuildTestArticle(
        this CmsContext context,
        string? slug = null,
        bool published = false,
        List<Tag>? tags = null,
        Guid? pageId = null
    )
    {
        var title = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 8);
        var article = new Article
        {
            Slug = slug ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 10),
            PublishedAt = published ? DateTime.UtcNow : null,
            Title = title,
            Description = title,
            Content = Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 16),
            Tags = tags ?? [],
            PageId = pageId
        };
        context.Articles.Add(article);
        context.SaveChanges();
        return article;
    }
}
