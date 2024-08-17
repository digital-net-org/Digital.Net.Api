using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace SafariDigital.Core.Files;

public static class ImageUtils
{
    public static bool IsImage(this IFormFile form) => form.ContentType.Contains("image");

    public static IFormFile CompressImage(this IFormFile form, int? quality = null)
    {
        using var ms = form.OpenReadStream();
        using var image = Image.Load(ms);
        using var memStream = new MemoryStream();
        var encoder = new JpegEncoder { Quality = quality ?? 75 };
        image.Save(memStream, encoder);

        // FIXME => Not working as intended, should manage bytes and use that function the Document service
        var result = new FormFile(memStream, 0, memStream.ToArray().Length, "compressed", "compressed.jpg")
        {
            ContentType = "image/jpeg"
        };

        return result;
    }
}