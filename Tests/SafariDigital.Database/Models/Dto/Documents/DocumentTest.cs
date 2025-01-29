using Digital.Net.Core.Models;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Documents;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Database.Models.Dto.Documents;

public class DocumentTest
{
    [Fact]
    public void DocumentModel_ReturnsValidModel()
    {
        var document = DocumentFactoryUtils.CreateDocument();
        var documentModel = Mapper.MapFromConstructor<Document, DocumentModel>(document);
        Assert.NotNull(documentModel);
        Assert.IsType<DocumentModel>(documentModel);
        Assert.Equal(document.Id, documentModel.Id);
        Assert.Equal(document.FileName, documentModel.FileName);
        Assert.Equal(document.MimeType, documentModel.MimeType);
        Assert.Equal(document.FileSize, documentModel.FileSize);
        Assert.Equal(document.Uploader?.Id, documentModel.UploaderId);
        Assert.Equal(document.CreatedAt, documentModel.CreatedAt);
        Assert.Equal(document.UpdatedAt, documentModel.UpdatedAt);
    }
}