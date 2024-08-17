using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;

namespace SafariDigital.Core.Files;

public static class ImageUtils
{
    public static bool IsImage(this IFormFile form) => form.ContentType.Contains("image");

    public static IFormFile CompressImage(this IFormFile form, Func<IFormFile> callback, int? quality = null)
    {
        using var ms = form.OpenReadStream();
        using var image = Image.Load(ms);
        using var memStream = new MemoryStream();
        var encoder = new JpegEncoder { Quality = quality ?? 75 };
        image.Save(memStream, encoder);

        var result = new FormFile(memStream, 0, memStream.ToArray().Length, "compressed", "compressed.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        return result;
    }
}