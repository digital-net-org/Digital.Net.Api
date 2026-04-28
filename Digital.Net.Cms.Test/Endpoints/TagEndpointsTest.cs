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

public class TagEndpointsTest
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
    public async Task CreateTag_ShouldCreateTag()
    {
        var client = await CreateAuthenticatedClientAsync();
        var payload = new TagPayload { Name = "NewTag", Color = "#ff0000" };

        var response = await client.CreateTag(payload);
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task GetTagById_ShouldReturnTag()
    {
        var client = await CreateAuthenticatedClientAsync();
        var tag = ApplicationFixture.GetCmsContext().BuildTestTag("FindById");

        var response = await client.GetTagById(tag.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<TagDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(tag.Id);
        await Assert.That(result.Value.Name).IsEqualTo(tag.Name);
    }

    [Test]
    public async Task GetTagById_ShouldReturnNotFound_WhenTagDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetTagById(Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetTags_ShouldReturnPaginatedTags()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = ApplicationFixture.GetCmsContext();
        context.BuildTestTag();
        context.BuildTestTag();
        context.BuildTestTag();

        var response = await client.GetTags(new TagQuery { Size = 2, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<TagDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Size).IsEqualTo(2);
        await Assert.That(result.Index).IsEqualTo(1);
        await Assert.That(result.Total).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task GetTags_ShouldFilterByName()
    {
        var client = await CreateAuthenticatedClientAsync();
        var context = ApplicationFixture.GetCmsContext();
        var target = context.BuildTestTag("FilterPrefix");
        context.BuildTestTag();

        var response = await client.GetTags(new TagQuery { Name = "FilterPrefix" });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<TagDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value.Any(t => t.Id == target.Id)).IsTrue();
        await Assert.That(result.Value.All(t => t.Name.StartsWith("FilterPrefix"))).IsTrue();
    }

    [Test]
    public async Task PatchTag_ShouldUpdateTag()
    {
        var client = await CreateAuthenticatedClientAsync();
        var tag = ApplicationFixture.GetCmsContext().BuildTestTag();

        var patch = new[] { new { op = "replace", path = "/Name", value = "UpdatedTagName" } };
        var response = await client.PatchTag(tag.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetTagById(tag.Id);
        var result = await getResponse.Content.ReadFromJsonAsync<Result<TagDto>>();
        await Assert.That(result!.Value!.Name).IsEqualTo("UpdatedTagName");
    }

    [Test]
    public async Task DeleteTag_ShouldDeleteTag()
    {
        var client = await CreateAuthenticatedClientAsync();
        var tag = ApplicationFixture.GetCmsContext().BuildTestTag();

        var response = await client.DeleteTag(tag.Id);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetTagById(tag.Id);
        await Assert.That(getResponse.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteTag_ShouldReturnNotFound_WhenTagDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.DeleteTag(Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }
}
