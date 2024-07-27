using SafariDigital.DataIdentities.Models.Document;
using Tests.Core.Factories;

namespace Tests.Unit.SafariDigital.Database.Models.DocumentTable;

public class DocumentTest
{
    [Fact]
    public void GetModel_ReturnsCorrectModelType()
    {
        // Arrange
        var document = DocumentFactory.CreateDocument();

        // Act
        var documentModel = document.GetModel<DocumentModel>();

        // Assert
        Assert.NotNull(documentModel);
        Assert.IsType<DocumentModel>(documentModel);
        Assert.Equal(document.Id, documentModel.Id);
        Assert.Equal(document.FileName, documentModel.FileName);
        Assert.Equal(document.DocumentType, documentModel.DocumentType);
        Assert.Equal(document.MimeType, documentModel.MimeType);
        Assert.Equal(document.FileSize, documentModel.FileSize);
        Assert.Equal(document?.Uploader?.Id, documentModel.UploaderId);
        Assert.Equal(document.CreatedAt, documentModel.CreatedAt);
        Assert.Equal(document.UpdatedAt, documentModel.UpdatedAt);
    }
}