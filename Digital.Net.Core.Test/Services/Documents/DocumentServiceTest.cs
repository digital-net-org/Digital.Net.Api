using Digital.Net.Core.Services.Documents;
using Digital.Net.Core.Services.Documents.Exceptions;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Digital.Net.Core.Test.Services.Documents;

public class DocumentServiceTest : UnitTest
{
    private static DigitalContext GetInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<DigitalContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DigitalContext(options);
    }
    [Test]
    public async Task GetDocumentFile_Should_Return_Error_When_Document_Is_Not_Found()
    {
        await using var context = GetInMemoryContext();
        var configMock = new Mock<IConfiguration>();
        var service = new DocumentService(context, configMock.Object);
        var docId = Guid.NewGuid();

        var result = service.GetDocumentFile(docId);

        await Assert.That(result.HasErrorOfType<ResourceNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task RemoveDocumentAsync_Should_Return_Error_When_Document_Is_Not_Found()
    {
        await using var context = GetInMemoryContext();
        var configMock = new Mock<IConfiguration>();
        var service = new DocumentService(context, configMock.Object);
        var docId = Guid.NewGuid();

        var result = await service.RemoveDocumentAsync(docId);

        await Assert.That(result.HasErrorOfType<DocumentNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task SaveDocumentAsync_Should_Return_Error_When_Uploader_Is_Null()
    {
        await using var context = GetInMemoryContext();
        var configMock = new Mock<IConfiguration>();
        var formFileMock = new Mock<IFormFile>();
        var service = new DocumentService(context, configMock.Object);

        var result = await service.SaveDocumentAsync(formFileMock.Object, null);

        await Assert.That(result.HasErrorOfType<NoUploaderException>()).IsTrue();
    }
}