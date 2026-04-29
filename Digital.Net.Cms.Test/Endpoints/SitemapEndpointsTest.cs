using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

public class SitemapEndpointsTest
{
    [ClassDataSource<ApplicationFixture>]
    public required ApplicationFixture ApplicationFixture { get; init; }

    [Test]
    public async Task GetSitemapData_ShouldReturnPublishedAndIndexedPages()
    {
        var client = ApplicationFixture.CreateApplicationClient();
        var context = ApplicationFixture.GetCmsContext();

        var included = context.BuildTestPage(published: true, indexed: true);
        context.BuildTestPage(published: true, indexed: false);
        context.BuildTestPage(published: false);

        var response = await client.GetSitemapData();
        var result = await response.Content.ReadFromJsonAsync<Result<List<SitemapEntryDto>>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Any(e => e.Path == included.Path)).IsTrue();
        await Assert.That(result.Value!.Count).IsEqualTo(1);
    }

    [Test]
    public async Task GetSitemapData_ShouldIncludePublishedArticles()
    {
        var client = ApplicationFixture.CreateApplicationClient();
        var context = ApplicationFixture.GetCmsContext();
        var article = context.BuildTestArticle(published: true);

        var response = await client.GetSitemapData();
        var result = await response.Content.ReadFromJsonAsync<Result<List<SitemapEntryDto>>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Any(e => e.Path == article.Path)).IsTrue();
    }
}