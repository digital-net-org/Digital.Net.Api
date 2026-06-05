using System.Net;
using System.Net.Http.Json;
using Digital.Net.Core;
using Digital.Net.Lib.Messages;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Digital.Net.Tests.Core.Factories.Data.Records;
using Digital.Net.Tests.Core.Sdk;

namespace Digital.Net.Tests.Cms.Http.Endpoints;

public class MediaEndpointsTest
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

    private string GetStoragePath() =>
        ApplicationFixture.GetConfiguration<string>(CoreSettings.FileSystemPathKey) ?? ".test_storage";


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


    [Test]
    public async Task GetMediaImage_ShouldServeOriginalImage()
    {
        var media = ApplicationFixture
            .GetCmsContext()
            .BuildTestMedia(ApplicationFixture.GetContext(), published: true, storagePath: GetStoragePath());
        var client = ApplicationFixture.CreateApplicationClient();
        var response = await client.GetMediaImage(media.Id, "png");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/png");
    }

    [Test]
    public async Task GetMediaImage_ShouldServeResizedVariant()
    {
        var media = ApplicationFixture.GetCmsContext()
            .BuildTestMedia(
                ApplicationFixture.GetContext(),
                published: true,
                storagePath: GetStoragePath(),
                imageWidth: 800,
                imageHeight: 600
            );
        var client = ApplicationFixture.CreateApplicationClient();
        var response = await client.GetMediaImage(media.Id, "webp", 400, 80);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/webp");
    }

    [Test]
    public async Task GetMediaImage_ShouldServeOriginal_WhenRequestedWidthIsLarger()
    {
        var media = ApplicationFixture.GetCmsContext()
            .BuildTestMedia(
                ApplicationFixture.GetContext(),
                published: true,
                storagePath: GetStoragePath(),
                imageWidth: 100,
                imageHeight: 100
            );
        var client = ApplicationFixture.CreateApplicationClient();
        var response = await client.GetMediaImage(media.Id, "png", 500);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/png");
    }

    [Test]
    public async Task GetMediaImage_ShouldReturnNotFound_WhenUnpublishedWithApplicationAuth()
    {
        var media = ApplicationFixture
            .GetCmsContext()
            .BuildTestMedia(ApplicationFixture.GetContext(), published: false, storagePath: GetStoragePath());
        var client = ApplicationFixture.CreateApplicationClient();
        var response = await client.GetMediaImage(media.Id, "png");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task GetMediaImage_ShouldServeUnpublished_WhenAuthenticatedWithJwt()
    {
        var media = ApplicationFixture
            .GetCmsContext()
            .BuildTestMedia(ApplicationFixture.GetContext(), published: false, storagePath: GetStoragePath());
        var client = await CreateAuthenticatedClientAsync();
        var response = await client.GetMediaImage(media.Id, "png");

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
    }

    [Test]
    public async Task GetMediaImage_ShouldServeSvgWithoutProcessing()
    {
        var media = ApplicationFixture
            .GetCmsContext()
            .BuildTestSvgMedia(ApplicationFixture.GetContext(), published: true, storagePath: GetStoragePath());
        var client = ApplicationFixture.CreateApplicationClient();
        var response = await client.GetMediaImage(media.Id, "svg", 50, 50);

        await Assert.That(response.StatusCode).EqualTo(HttpStatusCode.OK);
        await Assert.That(response.Content.Headers.ContentType!.MediaType).IsEqualTo("image/svg+xml");
    }


    [Test]
    public async Task PurgeMediaVariants_ShouldPurgeAllVariantsForMedia()
    {
        var media = ApplicationFixture.GetCmsContext()
            .BuildTestMedia(
                ApplicationFixture.GetContext(),
                published: true,
                storagePath: GetStoragePath(),
                imageWidth: 800,
                imageHeight: 600
            );
        var appClient = ApplicationFixture.CreateApplicationClient();
        await appClient.GetMediaImage(media.Id, "webp", 200, 80);
        await appClient.GetMediaImage(media.Id, "webp", 400, 80);

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