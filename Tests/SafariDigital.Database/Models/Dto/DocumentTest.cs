using Safari.Net.Core.Models;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Database.Models.Dto;

public class DocumentTest
{
    [Fact]
    public void DocumentModel_ReturnsValidModel()
    {
        var document = DocumentFactoryUtils.CreateDocument();
        var documentModel = Mapper.Map<Document, DocumentModel>(document);
        Assert.NotNull(documentModel);
        Assert.IsType<DocumentModel>(documentModel);
        Assert.Equal(document.Id, documentModel.Id);
        Assert.Equal(document.FileName, documentModel.FileName);
        Assert.Equal(document.DocumentType, documentModel.DocumentType);
        Assert.Equal(document.MimeType, documentModel.MimeType);
        Assert.Equal(document.FileSize, documentModel.FileSize);
        Assert.Equal(document.Uploader?.Id, documentModel.UploaderId);
        Assert.Equal(document.CreatedAt, documentModel.CreatedAt);
        Assert.Equal(document.UpdatedAt, documentModel.UpdatedAt);
    }
}