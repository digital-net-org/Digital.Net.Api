using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Tests.Core.Factories;
using Microsoft.AspNetCore.Http;
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
        _service = new DocumentService(_context, configMock.Object);
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
}