using Safari.Net.Core.Random;
using SafariDigital.Data.Models.Database;

namespace Tests.Utils.Factories;

public static class DocumentFactoryUtils
{
    public static Document CreateDocument() =>
        new()
        {
            FileName = Guid.NewGuid() + ".png",
            DocumentType = EDocumentType.Avatar,
            MimeType = EMimeType.Png,
            FileSize = Randomizer.GenerateRandomInt(),
            Uploader = null
        };
}