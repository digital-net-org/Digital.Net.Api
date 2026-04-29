using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Services.Pages.Dto;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

public class PageOpenGraphPatchTest
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

    private static async Task<List<OpenGraphEntryDto>> FetchOpenGraph(HttpClient client, Guid pageId)
    {
        var response = await client.GetPageOpenGraphForEdit(pageId);
        var result = await response.Content.ReadFromJsonAsync<Result<List<OpenGraphEntryDto>>>();
        return result!.Value!;
    }

    [Test]
    public async Task PatchPage_ShouldCreateOpenGraphEntries_WhenNoIdProvided()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var patch = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[]
                {
                    new { property = "og:title", content = "Hello" },
                    new { property = "og:image", content = "https://example.com/a.jpg" }
                }
            }
        };
        var response = await client.PatchPage(page.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var entries = await FetchOpenGraph(client, page.Id);
        await Assert.That(entries.Count).IsEqualTo(2);
        await Assert.That(entries[0].Property).IsEqualTo("og:title");
        await Assert.That(entries[0].Content).IsEqualTo("Hello");
    }

    [Test]
    public async Task PatchPage_ShouldUpdateExistingEntry_WhenIdMatches()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var seed = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[] { new { property = "og:title", content = "First" } }
            }
        };
        await client.PatchPage(page.Id, seed);

        var seeded = await FetchOpenGraph(client, page.Id);
        var existingId = seeded[0].Id;

        var update = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[] { new { id = existingId, property = "og:title", content = "Renamed" } }
            }
        };
        var response = await client.PatchPage(page.Id, update);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var after = await FetchOpenGraph(client, page.Id);
        await Assert.That(after.Count).IsEqualTo(1);
        await Assert.That(after[0].Id).IsEqualTo(existingId);
        await Assert.That(after[0].Content).IsEqualTo("Renamed");
    }

    [Test]
    public async Task PatchPage_ShouldDeleteRemovedEntry_WhenAbsentFromArray()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var seed = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[]
                {
                    new { property = "og:title", content = "Keep" },
                    new { property = "og:description", content = "Drop" }
                }
            }
        };
        await client.PatchPage(page.Id, seed);

        var seeded = await FetchOpenGraph(client, page.Id);
        var keepId = seeded.First(e => e.Property == "og:title").Id;

        var update = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[] { new { id = keepId, property = "og:title", content = "Keep" } }
            }
        };
        await client.PatchPage(page.Id, update);

        var after = await FetchOpenGraph(client, page.Id);
        await Assert.That(after.Count).IsEqualTo(1);
        await Assert.That(after[0].Property).IsEqualTo("og:title");
    }

    [Test]
    public async Task PatchPage_ShouldClearOpenGraph_WhenArrayIsEmpty()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        await client.PatchPage(page.Id, new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[] { new { property = "og:title", content = "X" } }
            }
        });

        await client.PatchPage(page.Id, new object[]
        {
            new { op = "replace", path = "/openGraph", value = Array.Empty<object>() }
        });

        var after = await FetchOpenGraph(client, page.Id);
        await Assert.That(after.Count).IsEqualTo(0);
    }

    [Test]
    public async Task PatchPage_ShouldRespectOrder_FromArrayIndex()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var patch = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[]
                {
                    new { property = "og:image", content = "C" },
                    new { property = "og:title", content = "A" },
                    new { property = "og:description", content = "B" }
                }
            }
        };
        await client.PatchPage(page.Id, patch);

        var entries = await FetchOpenGraph(client, page.Id);
        await Assert.That(entries.Select(e => e.Content).ToList())
            .IsEquivalentTo(new[] { "C", "A", "B" });
    }

    [Test]
    public async Task PatchPage_ShouldRejectUnknownProperty()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var patch = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[] { new { property = "og:nonexistent", content = "x" } }
            }
        };
        var response = await client.PatchPage(page.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    [Test]
    public async Task PatchPage_ShouldAcceptDuplicateProperties()
    {
        // The allowMultiple rule was dropped — duplicates are now accepted.
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var patch = new object[]
        {
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[]
                {
                    new { property = "og:title", content = "First" },
                    new { property = "og:title", content = "Second" }
                }
            }
        };
        var response = await client.PatchPage(page.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var entries = await FetchOpenGraph(client, page.Id);
        await Assert.That(entries.Count).IsEqualTo(2);
    }

    [Test]
    public async Task PatchPage_ShouldApplyTitleAndOpenGraph_InSameRequest()
    {
        var client = await CreateAuthenticatedClientAsync();
        var page = ApplicationFixture.GetCmsContext().BuildTestPage();

        var patch = new object[]
        {
            new { op = "replace", path = "/Title", value = "Combined" },
            new
            {
                op = "replace",
                path = "/openGraph",
                value = new[] { new { property = "og:title", content = "X" } }
            }
        };
        var response = await client.PatchPage(page.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var get = await client.GetPageById(page.Id);
        var dto = (await get.Content.ReadFromJsonAsync<Result<PageDto>>())!.Value!;
        await Assert.That(dto.Title).IsEqualTo("Combined");

        var entries = await FetchOpenGraph(client, page.Id);
        await Assert.That(entries.Count).IsEqualTo(1);
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

        var sheetsResponse = await client.GetPageSheetsForEdit(page.Id);
        var sheetsResult = await sheetsResponse.Content.ReadFromJsonAsync<Result<List<PageSheetPayloadDto>>>();
        await Assert.That(sheetsResult!.Value!.Count).IsEqualTo(1);

        var ogEntries = await FetchOpenGraph(client, page.Id);
        await Assert.That(ogEntries.Count).IsEqualTo(1);
    }
}
