using System.Net;
using System.Net.Http.Json;
using Digital.Net.Cms.Endpoints.Dto;
using Digital.Net.Core.Services.Pagination;
using Digital.Net.Lib.Messages;
using Digital.Net.Lib.Settings;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Cms.Test.Endpoints;

public class MediaEndpointsTest
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

    private string GetStoragePath() =>
        Application.GetConfiguration<string>(AppSettings.FileSystemPathKey) ?? ".test_storage";

    // --- Upload Tests ---

    [Test]
    public async Task UploadMedia_ShouldCreateMedia()
    {
        var client = await CreateAuthenticatedClientAsync();
        var (bytes, fileName, contentType) = TestMediaDataFactory.GenerateTestImage();

        var response = await client.UploadMedia(bytes, fileName, contentType, "TestImage", "alt text");
        var result = await response.Content.ReadFromJsonAsync<Result<Guid>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value).IsNotEqualTo(Guid.Empty);
    }

    [Test]
    public async Task UploadMedia_ShouldRejectUnsupportedFormat()
    {
        var client = await CreateAuthenticatedClientAsync();
        var bytes = "not an image"u8.ToArray();

        var response = await client.UploadMedia(bytes, "test.txt", "text/plain", "BadFile");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.BadRequest);
    }

    // --- CRUD Tests ---

    [Test]
    public async Task GetMediaById_ShouldReturnMedia()
    {
        var client = await CreateAuthenticatedClientAsync();
        var media = Application.GetCmsContext()
            .BuildTestMedia(Application.GetContext(), name: "FindById");

        var response = await client.GetMediaById(media.Id);
        var result = await response.Content.ReadFromJsonAsync<Result<MediaDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Value!.Id).IsEqualTo(media.Id);
        await Assert.That(result.Value.Name).IsEqualTo("FindById");
    }

    [Test]
    public async Task GetMediaById_ShouldReturnNotFound_WhenMediaDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetMediaById(Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetMedia_ShouldReturnPaginatedMedia()
    {
        var client = await CreateAuthenticatedClientAsync();
        var cmsCtx = Application.GetCmsContext();
        var digCtx = Application.GetContext();
        cmsCtx.BuildTestMedia(digCtx);
        cmsCtx.BuildTestMedia(digCtx);
        cmsCtx.BuildTestMedia(digCtx);

        var response = await client.GetMedia(new MediaQuery { Size = 2, Index = 1 });
        var result = await response.Content.ReadFromJsonAsync<QueryResult<MediaDto>>();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(result!.Size).IsEqualTo(2);
        await Assert.That(result.Index).IsEqualTo(1);
        await Assert.That(result.Total).IsGreaterThanOrEqualTo(3);
    }

    [Test]
    public async Task PatchMedia_ShouldUpdateMedia()
    {
        var client = await CreateAuthenticatedClientAsync();
        var media = Application.GetCmsContext()
            .BuildTestMedia(Application.GetContext());

        var patch = new[] { new { op = "replace", path = "/Name", value = "UpdatedName" } };
        var response = await client.PatchMedia(media.Id, patch);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetMediaById(media.Id);
        var result = await getResponse.Content.ReadFromJsonAsync<Result<MediaDto>>();
        await Assert.That(result!.Value!.Name).IsEqualTo("UpdatedName");
    }

    [Test]
    public async Task DeleteMedia_ShouldDeleteMedia()
    {
        var client = await CreateAuthenticatedClientAsync();
        var media = Application.GetCmsContext()
            .BuildTestMedia(Application.GetContext(), storagePath: GetStoragePath());

        var response = await client.DeleteMedia(media.Id);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);

        var getResponse = await client.GetMediaById(media.Id);
        await Assert.That(getResponse.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteMedia_ShouldReturnNotFound_WhenMediaDoesNotExist()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.DeleteMedia(Guid.NewGuid());

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    // --- Image Serving Tests ---

    [Test]
    public async Task GetMediaImage_ShouldServeOriginalImage()
    {
        var media = Application.GetCmsContext()
            .BuildTestMedia(Application.GetContext(), published: true, storagePath: GetStoragePath());
        var client = Application.CreateApplicationClient();

        var response = await client.GetMediaImage(media.Id, "png");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/png");
    }

    [Test]
    public async Task GetMediaImage_ShouldServeResizedVariant()
    {
        var media = Application.GetCmsContext()
            .BuildTestMedia(
                Application.GetContext(),
                published: true,
                storagePath: GetStoragePath(),
                imageWidth: 800,
                imageHeight: 600
            );
        var client = Application.CreateApplicationClient();

        var response = await client.GetMediaImage(media.Id, "webp", w: 400, q: 80);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/webp");
    }

    [Test]
    public async Task GetMediaImage_ShouldServeOriginal_WhenRequestedWidthIsLarger()
    {
        var media = Application.GetCmsContext()
            .BuildTestMedia(
                Application.GetContext(),
                published: true,
                storagePath: GetStoragePath(),
                imageWidth: 100,
                imageHeight: 100
            );
        var client = Application.CreateApplicationClient();

        var response = await client.GetMediaImage(media.Id, "png", w: 500);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/png");
    }

    [Test]
    public async Task GetMediaImage_ShouldReturnNotFound_WhenUnpublishedWithApplicationAuth()
    {
        var media = Application.GetCmsContext()
            .BuildTestMedia(Application.GetContext(), published: false, storagePath: GetStoragePath());
        var client = Application.CreateApplicationClient();

        var response = await client.GetMediaImage(media.Id, "png");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetMediaImage_ShouldServeUnpublished_WhenAuthenticatedWithJwt()
    {
        var media = Application.GetCmsContext()
            .BuildTestMedia(Application.GetContext(), published: false, storagePath: GetStoragePath());
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.GetMediaImage(media.Id, "png");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetMediaImage_ShouldServeSvgWithoutProcessing()
    {
        var media = Application.GetCmsContext()
            .BuildTestSvgMedia(Application.GetContext(), published: true, storagePath: GetStoragePath());
        var client = Application.CreateApplicationClient();

        var response = await client.GetMediaImage(media.Id, "svg", w: 50, q: 50);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/svg+xml");
    }

    // --- Purge Tests ---

    [Test]
    public async Task PurgeMediaVariants_ShouldPurgeAllVariantsForMedia()
    {
        var media = Application.GetCmsContext()
            .BuildTestMedia(
                Application.GetContext(),
                published: true,
                storagePath: GetStoragePath(),
                imageWidth: 800,
                imageHeight: 600
            );
        var appClient = Application.CreateApplicationClient();
        await appClient.GetMediaImage(media.Id, "webp", w: 200, q: 80);
        await appClient.GetMediaImage(media.Id, "webp", w: 400, q: 80);

        var client = await CreateAuthenticatedClientAsync();
        var response = await client.PurgeMediaVariants(media.Id);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task PurgeAllVariants_ShouldPurgeEverything()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PurgeAllVariants();

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }
}
