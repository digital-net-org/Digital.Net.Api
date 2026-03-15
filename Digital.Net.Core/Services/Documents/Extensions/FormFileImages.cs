using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;

namespace Digital.Net.Core.Services.Documents.Extensions;

public static class FormFileImages
{
    public static bool IsImage(this IFormFile form) => form.ContentType.Contains("image");

    public static async Task<Result<IFormFile>> CompressImageAsync(
        this IFormFile form,
        string? fileName = null,
        int? quality = null
    )
    {
        var result = new Result<IFormFile>();
        try
        {
            await using var ms = form.OpenReadStream();
            using var image = await Image.LoadAsync(ms);
            var memStream = new MemoryStream();
            await image.SaveAsync(memStream, FormFileHelper.GetJpegEncoder(quality));
            var fileBytes = memStream.ToArray();
            fileName ??= FormFileHelper.GenerateFileName();

            result.Value = new FormFile(new MemoryStream(fileBytes), 0, fileBytes.Length, fileName, $"{fileName}.jpg")
            {
                Headers = new HeaderDictionary(),
                ContentType = "image/jpeg"
            };
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}