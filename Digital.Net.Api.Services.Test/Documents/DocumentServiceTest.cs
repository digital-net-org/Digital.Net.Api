using Digital.Net.Api.Core.Exceptions.types;
using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Repositories;
using Digital.Net.Api.Services.Documents;
using Digital.Net.Api.Services.Documents.Exceptions;
using Digital.Net.Tests.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Digital.Net.Api.Services.Test.Documents;

public class DocumentServiceTest : UnitTest
{
    [Test]
    public async Task GetDocumentFile_Should_Return_Error_When_Document_Is_Not_Found()
    {
        var docRepoMock = new Mock<IRepository<Document>>();
        var configMock = new Mock<IConfiguration>();
        var service = new DocumentService(docRepoMock.Object, configMock.Object);
        var docId = Guid.NewGuid();

        docRepoMock.Setup(r => r.GetById(docId)).Returns((Document)null!);
        var result = service.GetDocumentFile(docId);

        await Assert.That(result.HasErrorOfType<ResourceNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task RemoveDocumentAsync_Should_Return_Error_When_Document_Is_Not_Found()
    {
        var docRepoMock = new Mock<IRepository<Document>>();
        var configMock = new Mock<IConfiguration>();
        var service = new DocumentService(docRepoMock.Object, configMock.Object);
        var docId = Guid.NewGuid();

        docRepoMock.Setup(r => r.GetByIdAsync(docId)).ReturnsAsync((Document)null!);
        var result = await service.RemoveDocumentAsync(docId);

        await Assert.That(result.HasErrorOfType<DocumentNotFoundException>()).IsTrue();
    }

    [Test]
    public async Task SaveDocumentAsync_Should_Return_Error_When_Uploader_Is_Null()
    {
        var docRepoMock = new Mock<IRepository<Document>>();
        var configMock = new Mock<IConfiguration>();
        var formFileMock = new Mock<IFormFile>();
        var service = new DocumentService(docRepoMock.Object, configMock.Object);

        var result = await service.SaveDocumentAsync(formFileMock.Object, null);

        await Assert.That(result.HasErrorOfType<NoUploaderException>()).IsTrue();
    }
}