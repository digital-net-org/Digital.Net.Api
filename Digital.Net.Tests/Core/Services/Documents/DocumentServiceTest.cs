using Digital.Net.Core;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Tests.Core.Factories;
using Digital.Net.Tests.Core.Factories.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using TUnit.Core.Interfaces;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace Digital.Net.Tests.Core.Services.Documents;

public class DocumentServiceTest : UnitTest, IAsyncInitializer
{
    [ClassDataSource<DatabaseFixture>]
    public required DatabaseFixture DbFixture { get; init; }

    private DigitalContext _context = null!;
    private DocumentService _service = null!;

    public Task InitializeAsync()
    {
        _context = DbFixture.CreateContext<DigitalContext>();
        var configMock = new Mock<IConfiguration>();
        var extractorMock = new Mock<IDocumentDimensionExtractor>();
        _service = new DocumentService(_context, configMock.Object, extractorMock.Object);
        return Task.CompletedTask;
    }

    [Test]
    public async Task GetDocumentFile_Should_Return_Error_When_Document_Is_Not_Found()
    {
        var docId = Guid.NewGuid();
        var result = _service.GetDocumentFile(docId);
        await Assert.That(result.HasErrorOfType<ResourceNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task RemoveDocumentAsync_Should_Return_Error_When_Document_Is_Not_Found()
    {
        var docId = Guid.NewGuid();
        var result = await _service.RemoveDocumentAsync(docId);
        await Assert.That(result.HasErrorOfType<DocumentNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task SaveDocumentAsync_Should_Return_Error_When_Uploader_Is_Null()
    {
        var formFileMock = new Mock<IFormFile>();
        var result = await _service.SaveDocumentAsync(formFileMock.Object, null);
        await Assert.That(result.HasErrorOfType<NoUploaderException>()).IsTrue();
    }

    [Test]
    public async Task SaveDocumentAsync_Should_Populate_Width_And_Height_For_Image()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"digital_test_{Guid.NewGuid():N}");
        var service = BuildServiceWithRealPipeline(tempDir);

        try
        {
            var user = _context.BuildTestUser();
            var (bytes, fileName, contentType) = TestMediaDataFactory.GenerateTestImage(120, 80);
            var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", fileName)
            {
                Headers = new HeaderDictionary(),
                ContentType = contentType
            };

            var result = await service.SaveDocumentAsync(file, user);

            await Assert.That(result.HasError).IsFalse();
            await Assert.That(result.Value).IsNotNull();

            var saved = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == result.Value!.Id);
            await Assert.That(saved).IsNotNull();
            await Assert.That(saved!.Width).IsEqualTo(120);
            await Assert.That(saved.Height).IsEqualTo(80);
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    [Test]
    public async Task SaveDocumentAsync_Should_Leave_Width_And_Height_Null_For_Non_Image()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), $"digital_test_{Guid.NewGuid():N}");
        var service = BuildServiceWithRealPipeline(tempDir);

        try
        {
            var user = _context.BuildTestUser();
            var bytes = "not an image"u8.ToArray();
            var file = new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", "test.txt")
            {
                Headers = new HeaderDictionary(),
                ContentType = "text/plain"
            };

            var result = await service.SaveDocumentAsync(file, user);

            await Assert.That(result.HasError).IsFalse();
            await Assert.That(result.Value).IsNotNull();

            var saved = await _context.Documents.AsNoTracking().FirstOrDefaultAsync(d => d.Id == result.Value!.Id);
            await Assert.That(saved).IsNotNull();
            await Assert.That(saved!.Width).IsNull();
            await Assert.That(saved.Height).IsNull();
        }
        finally
        {
            if (Directory.Exists(tempDir))
                Directory.Delete(tempDir, true);
        }
    }

    private DocumentService BuildServiceWithRealPipeline(string storagePath)
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                [CoreSettings.FileSystemPathKey] = storagePath
            })
            .Build();
        return new DocumentService(_context, config, new DocumentDimensionExtractor());
    }
}
