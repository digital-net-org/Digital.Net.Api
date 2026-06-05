using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Http.Dto;
using Digital.Net.Cms.Models.Pages;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Cms.Http.Endpoints;

public class PageEndpointsTest
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

    private Page CreateTestPage(string? path = null, bool published = false) =>
        ApplicationFixture.GetCmsContext().BuildTestPage(path, published);

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
    public async Task PatchPage_ShouldOrchestrateSheetsAndOpenGraph_InSameTransaction()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var patch = new object[]
        {
            new
            {
                op = "replace",
                path = "/sheets",
                value = new[] { new { name = "main.css", type = "css", content = "body{}", published = true } }
            },
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[] { new { property = "og:title", content = "Title" } }
            }
        };
        var response = await client.PatchPage(page.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var sheetsResponse = await client.GetPageSheets(page.Id);
        var sheetsResult = await sheetsResponse.Content.ReadFromJsonAsync<Result<List<PageSheetPayloadDto>>>();
        await Assert.That(sheetsResult!.Value!.Count).IsEqualTo(1);

        var ogResponse = await client.GetPageOpenGraph(page.Id);
        var ogResult = await ogResponse.Content.ReadFromJsonAsync<Result<List<OpenGraphEntryDto>>>();
        await Assert.That(ogResult!.Value!.Count).IsEqualTo(1);
    }
}