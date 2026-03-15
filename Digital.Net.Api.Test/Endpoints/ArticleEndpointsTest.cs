using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Crud.Endpoints;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Api.Test.Endpoints;

public class ArticleEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var user = Application.CreateUser(new TestUserPayload { IsActive = true });
        var client = Application.CreateClient();
        await client.Login(user);
        return client;
    }

    [Test]
    public async Task GetArticles_ShouldFilterByTagId()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = Application.GetCmsContext();

        var tag = context.BuildTestTag();
        var articleWithTag = context.BuildTestArticle(tags: [tag]);
        var articleWithoutTag = context.BuildTestArticle();

        var response = await client.GetArticles(new ArticleQuery { TagId = tag.Id });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<ArticleDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.Any(a => a.Id == articleWithTag.Id)).IsTrue();
        await Assert.That(result.Value.Any(a => a.Id == articleWithoutTag.Id)).IsFalse();
    }
}
