using SafariDigital.Database.Models.DocumentTable;
using Tests.Core.Utils;

namespace Tests.Core.Factories;

public static class DocumentFactory
{
    public static Document CreateDocument() =>
        new()
        {
            FileName = Guid.NewGuid() + ".png",
            DocumentType = EDocumentType.Avatar,
            MimeType = EMimeType.Png,
            FileSize = RandomUtils.GenerateRandomInt(),
            Uploader = null
        };
}