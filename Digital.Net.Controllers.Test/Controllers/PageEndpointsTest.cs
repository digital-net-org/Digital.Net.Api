using System.Net;
using System.Net.Http.Json;
using Digital.Net.Controllers.Dto;
using Digital.Net.Core.Messages;
using Digital.Net.Entities.Crud.Controllers;
using Digital.Net.Entities.Models.Pages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Controllers.Test.Controllers;

public class PageEndpointsTest
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

    private Page CreateTestPage(string? title = null, string? path = null, bool isPublished = false) =>
        Application.GetRepository<Page>().BuildTestPage(title, path, isPublished);

    [Test]
    public async Task CreatePage_ShouldCreatePage()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new PagePayload
            { Title = "TestPage", Path = "/test-create-page-" + Guid.NewGuid().ToString("N")[..8] };

        var response = await client.CreatePage(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetPageById_ShouldReturnPage()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = CreateTestPage();

        var response = await client.GetPageById(page.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<PageDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(page.Id);
        await Assert.That(result.Value!.Title).IsEqualTo(page.Title);
    }

    [Test]
    public async Task GetPages_ShouldReturnPaginatedPages()
    {
        var client = await CreateAuthenticatedClientAsync();
        CreateTestPage();
        CreateTestPage();
        CreateTestPage();

        var response = await client.GetPages(new PageQuery { Size = 2, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<PageDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Size).IsEqualTo(2);
        await Assert.That(result.Index).IsEqualTo(1);
        await Assert.That(result.Total).IsGreaterThanOrEqualTo(3);
        await Assert.That(result.Count).IsGreaterThan(0);
    }

    [Test]
    public async Task GetPages_ShouldFilterByTitle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var target = CreateTestPage("UniqueFilterTitle");
        CreateTestPage();

        var response = await client.GetPages(new PageQuery { Title = "UniqueFilter" });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<PageDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Total).IsEqualTo(1);
        await Assert.That(result.Value.FirstOrDefault(p => p.Id == target.Id)).IsNotNull();
    }

    [Test]
    public async Task GetPages_ShouldFilterByIsPublished()
    {
        var client = await CreateAuthenticatedClientAsync();
        CreateTestPage(isPublished: true);
        CreateTestPage(isPublished: false);

        var response = await client.GetPages(new PageQuery { IsPublished = true });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<PageDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(p => p.IsPublished == true)).IsTrue();
    }
    
    [Test]
    public async Task PatchPage_ShouldUpdatePage()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = CreateTestPage();

        var patch = new[] { new { op = "replace", path = "/Title", value = "UpdatedTitle" } };
        var response = await client.PatchPage(page.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetPageById(page.Id);
        var result = await getResponse.Content.ReadFromJsonAsync<Result<PageDto>>();

        await Assert.That(result!.Value!.Title).IsEqualTo("UpdatedTitle");
    }

    [Test]
    public async Task PatchPage_ShouldRejectOverMaxLengthTitle()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = CreateTestPage();

        var tooLongTitle = new string('A', 65);
        var patch = new[] { new { op = "replace", path = "/Title", value = tooLongTitle } };
        var response = await client.PatchPage(page.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task DeletePage_ShouldDeletePage()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = CreateTestPage();

        var response = await client.DeletePage(page.Id);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetPageById(page.Id);
        await Assert.That(getResponse.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetPageByPath_ShouldReturnPublishedPage()
    {
        var client = Application.CreateClient();
        var uniquePath = "test-public-path-" + Guid.NewGuid().ToString("N")[..8];
        CreateTestPage(path: uniquePath, isPublished: true);

        var response = await client.GetPageByPath(uniquePath);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetPageByPath_ShouldReturnNotFound_WhenPageIsNotPublished()
    {
        var client = Application.CreateClient();
        var uniquePath = "test-unpublished-" + Guid.NewGuid().ToString("N")[..8];
        CreateTestPage(path: uniquePath, isPublished: false);

        var response = await client.GetPageByPath(uniquePath);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }
}