using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.Services.Pagination;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

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
    public async Task GetArticleById_ShouldReturnArticle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var article = ApplicationFixture.GetCmsContext().BuildTestArticle();

        var response = await client.GetArticleById(article.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<ArticleDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(article.Id);
        await Assert.That(result.Value.Name).IsEqualTo(article.Name);
    }

    [Test]
    public async Task GetArticleById_ShouldReturnNotFound_WhenArticleDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetArticleById(Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetArticles_ShouldReturnPaginatedArticles()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = ApplicationFixture.GetCmsContext();
        context.BuildTestArticle();
        context.BuildTestArticle();
        context.BuildTestArticle();

        var response = await client.GetArticles(new ArticleQuery { Size = 2, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<ArticleDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Size).IsEqualTo(2);
        await Assert.That(result.Index).IsEqualTo(1);
        await Assert.That(result.Total).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task GetArticles_ShouldFilterByPublished()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = ApplicationFixture.GetCmsContext();
        context.BuildTestArticle(published: true);
        context.BuildTestArticle(published: false);

        var response = await client.GetArticles(new ArticleQuery { Published = true });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<ArticleDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(a => a.Published)).IsTrue();
    }

    [Test]
    public async Task GetArticles_ShouldFilterByName()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = ApplicationFixture.GetCmsContext();
        var target = context.BuildTestArticle();
        context.BuildTestArticle();

        var response = await client.GetArticles(new ArticleQuery { Name = target.Name });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<ArticleDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.Any(a => a.Id == target.Id)).IsTrue();
        await Assert.That(result.Value.All(a => a.Name.StartsWith(target.Name))).IsTrue();
    }

    [Test]
    public async Task GetArticles_ShouldFilterByTagId()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = ApplicationFixture.GetCmsContext();

        var tag = context.BuildTestTag();
        var articleWithTag = context.BuildTestArticle(tags: [tag]);
        var articleWithoutTag = context.BuildTestArticle();

        var response = await client.GetArticles(new ArticleQuery { TagId = tag.Id });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<ArticleDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.Any(a => a.Id == articleWithTag.Id)).IsTrue();
        await Assert.That(result.Value.Any(a => a.Id == articleWithoutTag.Id)).IsFalse();
    }

    [Test]
    public async Task GetArticleByPath_ShouldReturnPublishedArticle()
    {
        var client = ApplicationFixture.CreateApplicationClient();
        var path = "/article-path-" + Guid.NewGuid().ToString("N")[..8];
        ApplicationFixture.GetCmsContext().BuildTestArticle(path, true);

        var response = await client.GetArticleByPath(path);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetArticleByPath_ShouldReturnNotFound_WhenNotPublished()
    {
        var client = ApplicationFixture.CreateApplicationClient();
        var path = "/article-unpublished-" + Guid.NewGuid().ToString("N")[..8];
        ApplicationFixture.GetCmsContext().BuildTestArticle(path, false);

        var response = await client.GetArticleByPath(path);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreateArticle_ShouldCreateArticle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new ArticlePayload
        {
            Path = "/new-article-" + Guid.NewGuid().ToString("N")[..8],
            Name = "Test Article",
            Content = "Test content body"
        };

        var response = await client.CreateArticle(payload);
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task PatchArticle_ShouldUpdateArticle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var article = ApplicationFixture.GetCmsContext().BuildTestArticle();

        var patch = new[] { new { op = "replace", path = "/Name", value = "UpdatedName" } };
        var response = await client.PatchArticle(article.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetArticleById(article.Id);
        var result = await getResponse.Content.ReadFromJsonAsync<Result<ArticleDto>>();
        await Assert.That(result!.Value!.Name).IsEqualTo("UpdatedName");
    }

    [Test]
    public async Task DeleteArticle_ShouldDeleteArticle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var article = ApplicationFixture.GetCmsContext().BuildTestArticle();

        var response = await client.DeleteArticle(article.Id);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetArticleById(article.Id);
        await Assert.That(getResponse.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }
}
