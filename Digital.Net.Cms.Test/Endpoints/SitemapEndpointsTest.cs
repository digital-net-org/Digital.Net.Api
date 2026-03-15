using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

public class SitemapEndpointsTest
{
    [ClassDataSource<TestApplication>]
    public required TestApplication Application { get; init; }

    [Test]
    public async Task GetSitemapData_ShouldRequireApplicationAuth()
    {
        var client = Application.CreateClient();

        var response = await client.GetSitemapData();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }

    [Test]
    public async Task GetSitemapData_ShouldReturnPublishedAndIndexedPages()
    {
        var client = Application.CreateApplicationClient();
        var context = Application.GetCmsContext();
        var included = context.BuildTestPage(published: true);
        context.BuildTestPage(published: false); // excluded

        var response = await client.GetSitemapData();
        var result = await response.Content.ReadFromJsonAsync<List<SitemapEntryDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Any(e => e.Path == included.Path)).IsTrue();
    }

    [Test]
    public async Task GetSitemapData_ShouldExcludeUnpublishedPages()
    {
        var client = Application.CreateApplicationClient();
        var context = Application.GetCmsContext();
        var unpublished = context.BuildTestPage(published: false);

        var response = await client.GetSitemapData();
        var result = await response.Content.ReadFromJsonAsync<List<SitemapEntryDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Any(e => e.Path == unpublished.Path)).IsFalse();
    }

    [Test]
    public async Task GetSitemapData_ShouldIncludePublishedArticles()
    {
        var client = Application.CreateApplicationClient();
        var context = Application.GetCmsContext();
        var article = context.BuildTestArticle(published: true);

        var response = await client.GetSitemapData();
        var result = await response.Content.ReadFromJsonAsync<List<SitemapEntryDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        // Articles inherit from Page and should appear in the sitemap
        await Assert.That(result!.Any(e => e.Path == article.Path)).IsTrue();
    }
}
