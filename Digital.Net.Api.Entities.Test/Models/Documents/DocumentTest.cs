using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Models.Documents;

namespace Digital.Net.Api.Entities.Test.Models.Documents;

public class DocumentTest
{
    public static readonly Document TestDocument = new()
    {
        FileName = Guid.NewGuid() + ".png",
        MimeType = "image/png",
        FileSize = Randomizer.GenerateRandomInt(),
        Uploader = null
    };

    [Fact]
    public void DocumentModel_ReturnsValidModel()
    {
        var dto = Mapper.MapFromConstructor<Document, DocumentDto>(TestDocument);
        Assert.NotNull(dto);
        Assert.IsType<DocumentDto>(dto);
        Assert.Equal(TestDocument.Id, dto.Id);
        Assert.Equal(TestDocument.FileName, dto.FileName);
        Assert.Equal(TestDocument.MimeType, dto.MimeType);
        Assert.Equal(TestDocument.FileSize, dto.FileSize);
        Assert.Equal(TestDocument.Uploader?.Id, dto.UploaderId);
        Assert.Equal(TestDocument.CreatedAt, dto.CreatedAt);
        Assert.Equal(TestDocument.UpdatedAt, dto.UpdatedAt);
    }
}