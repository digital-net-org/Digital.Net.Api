using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.Services.Pagination;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Cms.Endpoints;

public class ArticleEndpointsTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var user = ApplicationFixture.CreateUser(new TestUserPayload { IsActive = true });
        var client = ApplicationFixture.CreateClient();
        await client.Login(user);
        return client;
    }

    [Test]
    public async Task GetArticles_ShouldPopulateTags_ViaInclude()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var tag1 = ctx.BuildTestTag("include-tag-1-" + Guid.NewGuid().ToString("N")[..6]);
        var tag2 = ctx.BuildTestTag("include-tag-2-" + Guid.NewGuid().ToString("N")[..6]);
        var article = ctx.BuildTestArticle(tags: [tag1, tag2]);

        var response = await client.GetArticles(new ArticleQuery { Size = 50, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<ArticleDto>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var dto = result!.Value.First(a => a.Id == article.Id);
        await Assert.That(dto.Tags.Count).IsEqualTo(2);
        await Assert.That(dto.Tags.Any(t => t.Id == tag1.Id)).IsTrue();
        await Assert.That(dto.Tags.Any(t => t.Id == tag2.Id)).IsTrue();
    }

    [Test]
    public async Task GetArticles_ShouldFilterByTagId_ThroughPivotSkipNav()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ctx = ApplicationFixture.GetCmsContext();
        var targetTag = ctx.BuildTestTag("filter-tag-" + Guid.NewGuid().ToString("N")[..6]);
        var taggedArticle = ctx.BuildTestArticle(tags: [targetTag]);
        ctx.BuildTestArticle();

        var response = await client.GetArticles(new ArticleQuery { TagId = targetTag.Id, Size = 50, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<ArticleDto>>();

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(a => a.Tags.Any(t => t.Id == targetTag.Id))).IsTrue();
        await Assert.That(result.Value.Any(a => a.Id == taggedArticle.Id)).IsTrue();
    }
}