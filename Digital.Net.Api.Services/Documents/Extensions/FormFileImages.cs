using Digital.Net.Api.Core.Messages;
using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;

namespace Digital.Net.Api.Services.Documents.Extensions;

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
            using var memStream = new MemoryStream();
            await image.SaveAsync(memStream, FormFileHelper.GetJpegEncoder(quality));
            fileName ??= FormFileHelper.GenerateFileName();

            result.Value = new FormFile(memStream, 0, memStream.ToArray().Length, fileName, $"{fileName}.jpg")
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