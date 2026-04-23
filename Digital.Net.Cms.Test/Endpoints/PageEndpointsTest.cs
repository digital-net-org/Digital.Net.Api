using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Cms.Models;
using Digital.Net.Core.Services.Pagination;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

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

    private Page CreateTestPage(string? path = null, bool published = false) =>
        Application.GetCmsContext().BuildTestPage(path, published);

    [Test]
    public async Task CreatePage_ShouldCreatePage()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new PagePayload
            { Path = "/test-create-page-" + Guid.NewGuid().ToString("N")[..8] };

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
        await Assert.That(result.Value!.Path).IsEqualTo(page.Path);
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
    public async Task GetPages_ShouldFilterByPublished()
    {
        var client = await CreateAuthenticatedClientAsync();
        CreateTestPage(published: true);
        CreateTestPage(published: false);

        var response = await client.GetPages(new PageQuery { Published = true });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<PageDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.All(p => p.Published)).IsTrue();
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

        var tooLongTitle = new string('A', 257);
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
        var client = Application.CreateApplicationClient();
        var uniquePath = "/test-public-path-" + Guid.NewGuid().ToString("N")[..8];
        CreateTestPage(uniquePath, true);

        var response = await client.GetPageByPath(uniquePath);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetPageByPath_ShouldReturnNotFound_WhenPageIsNotPublished()
    {
        var client = Application.CreateApplicationClient();
        var uniquePath = "/test-unpublished-" + Guid.NewGuid().ToString("N")[..8];
        CreateTestPage(uniquePath, false);

        var response = await client.GetPageByPath(uniquePath);
        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task CreatePage_ShouldRejectInvalidPath()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new PagePayload { Path = "/invalid!path" };

        var response = await client.CreatePage(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreatePage_ShouldRejectTrailingSlash()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new PagePayload { Path = "/home/" };

        var response = await client.CreatePage(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreatePage_ShouldAcceptDynamicSlugPath()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new PagePayload
        {
            Path = "/articles-" + Guid.NewGuid().ToString("N")[..8] + "/:id"
        };

        var response = await client.CreatePage(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task CreatePage_ShouldRejectEntityTypeWithoutDynamicSlug()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new PagePayload
        {
            Path = "/static-" + Guid.NewGuid().ToString("N")[..8],
            EntityType = PageEntityType.Article
        };

        var response = await client.CreatePage(payload);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task CreatePage_ShouldAcceptEntityTypeWithDynamicSlug()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new PagePayload
        {
            Path = "/products-" + Guid.NewGuid().ToString("N")[..8] + "/:id",
            EntityType = PageEntityType.Article
        };

        var response = await client.CreatePage(payload);
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotDefault();

        var getResponse = await client.GetPageById(result.Value);
        var dto = await getResponse.Content.ReadFromJsonAsync<Result<PageDto>>();
        await Assert.That(dto!.Value!.EntityType).IsEqualTo(PageEntityType.Article);
    }

    [Test]
    public async Task PatchPage_ShouldRejectEntityTypeLostConsistency()
    {
        var client = await CreateAuthenticatedClientAsync();
        var basePath = "/rej-" + Guid.NewGuid().ToString("N")[..8] + "/:id";
        var createResp = await client.CreatePage(
            new PagePayload { Path = basePath, EntityType = PageEntityType.Article });
        var created = await createResp.Content.ReadFromJsonAsync<Result<Guid>>();

        var patch = new[]
        {
            new { op = "replace", path = "/Path", value = "/rej-" + Guid.NewGuid().ToString("N")[..8] }
        };
        var response = await client.PatchPage(created!.Value, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchPage_ShouldAcceptPathChangeWithEntityTypeClear()
    {
        var client = await CreateAuthenticatedClientAsync();
        var basePath = "/acc-" + Guid.NewGuid().ToString("N")[..8] + "/:id";
        var createResp = await client.CreatePage(
            new PagePayload { Path = basePath, EntityType = PageEntityType.Article });
        var created = await createResp.Content.ReadFromJsonAsync<Result<Guid>>();

        var newPath = "/acc-" + Guid.NewGuid().ToString("N")[..8];
        var patch = new object[]
        {
            new { op = "replace", path = "/Path", value = newPath },
            new { op = "replace", path = "/EntityType", value = (string?)null }
        };
        var response = await client.PatchPage(created!.Value, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetPathAvailability_ShouldReturnTrue_WhenPathIsFree()
    {
        var client = await CreateAuthenticatedClientAsync();
        var freePath = "/free-" + Guid.NewGuid().ToString("N")[..8];

        var response = await client.GetPathAvailability(freePath);
        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsTrue();
    }

    [Test]
    public async Task GetPathAvailability_ShouldReturnFalse_WhenPathIsTaken()
    {
        var client = await CreateAuthenticatedClientAsync();
        var takenPath = "/taken-" + Guid.NewGuid().ToString("N")[..8];
        CreateTestPage(takenPath);

        var response = await client.GetPathAvailability(takenPath);
        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsFalse();
    }

    [Test]
    public async Task GetPathAvailability_ShouldReturnTrue_WhenExcludeIdOwnsPath()
    {
        var client = await CreateAuthenticatedClientAsync();
        var ownPath = "/self-" + Guid.NewGuid().ToString("N")[..8];
        var page = CreateTestPage(ownPath);

        var response = await client.GetPathAvailability(ownPath, page.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<bool>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsTrue();
    }

    [Test]
    public async Task GetPathAvailability_ShouldRequireAuthentication()
    {
        var client = Application.CreateClient();

        var response = await client.GetPathAvailability("/any");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.Unauthorized);
    }
}
