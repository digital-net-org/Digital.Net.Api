using Digital.Lib.Net.Core.Random;
using SafariDigital.Data.Models.Documents;

namespace Tests.Utils.Factories;

public static class DocumentFactoryUtils
{
    public static Document CreateDocument() =>
        new()
        {
            FileName = Guid.NewGuid() + ".png",
            MimeType = "image/png",
            FileSize = Randomizer.GenerateRandomInt(),
            Uploader = null
        };
}