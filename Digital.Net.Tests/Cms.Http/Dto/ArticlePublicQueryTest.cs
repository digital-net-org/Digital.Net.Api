using Digital.Net.Cms.Http.Dto;
using Digital.Net.Core.Http.Services.Pagination;
using Digital.Net.Tests.Core;

namespace Digital.Net.Tests.Cms.Http.Dto;

public class ArticlePublicQueryTest : UnitTest
{
    [Test]
    public async Task ValidateParameters_TruncatesName_WhenAboveMaxLength()
    {
        var query = new ArticlePublicQuery { Name = new string('a', ArticlePublicQuery.MaxNameLength + 50) };
        query.ValidateParameters();
        await Assert.That(query.Name!.Length).IsEqualTo(ArticlePublicQuery.MaxNameLength);
    }

    [Test]
    public async Task ValidateParameters_KeepsName_WhenWithinMaxLength()
    {
        var query = new ArticlePublicQuery { Name = "safari digital" };
        query.ValidateParameters();
        await Assert.That(query.Name).IsEqualTo("safari digital");
    }

    [Test]
    public async Task ValidateParameters_StillNormalizesPagination()
    {
        var query = new ArticlePublicQuery { Index = -5, Size = 1_000_000 };
        query.ValidateParameters();
        await Assert.That(query.Index).IsEqualTo(Query.DefaultIndex);
        await Assert.That(query.Size).IsEqualTo(Query.MaxSize);
    }
}
