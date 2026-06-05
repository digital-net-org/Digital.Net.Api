using Digital.Net.Cms.Context;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core;
using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Core.Services.Documents;
using Digital.Net.Lib.Configuration;
using Digital.Net.Lib.Exceptions.types;
using Digital.Net.Lib.Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Digital.Net.Cms.Services;

public class MediaService(
    CmsContext cmsContext,
    DigitalContext digitalContext,
    IDocumentService documentService,
    IConfiguration configuration
)
{
    /// <summary>
    ///     Returns the Document to serve for a media image request.
    ///     If width/quality are provided and the media is a raster image,
    ///     returns an existing or newly generated variant Document.
    ///     For SVGs or requests without resize params, returns the original Document.
    /// </summary>
    /// <param name="mediaId">The media ID for which to retrieve the variant.</param>
    /// <param name="width">The optional width of the variant to generate.</param>
    /// <param name="quality">The optional quality of the variant to generate.</param>
    public async Task<Result<Document>> GetOrCreateVariantAsync(Guid mediaId, int? width, int? quality)
    {
        var result = new Result<Document>();
        var media = await cmsContext.Media
            .Include(m => m.Variants)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media is null)
            return result.AddError(new ResourceNotFoundException());

        var originalDocument = await digitalContext.Documents.FindAsync(media.DocumentId);
        if (originalDocument is null)
            return result.AddError(new ResourceNotFoundException());

        if (!width.HasValue || originalDocument.IsSvg())
        {
            result.Value = originalDocument;
            return result;
        }

        var q = Math.Clamp(quality ?? 100, 0, 100);
        var w = width.Value;

        var existingVariant = media.Variants
            .FirstOrDefault(v => v.Width == w && v.Quality == q);

        if (existingVariant is not null)
        {
            var variantDocument = await digitalContext.Documents.FindAsync(existingVariant.DocumentId);
            if (variantDocument is not null)
            {
                result.Value = variantDocument;
                return result;
            }
        }

        return await GenerateVariantAsync(media, originalDocument, w, q);
    }

    /// <summary>
    ///     Purges all cached variants for a given media.
    /// </summary>
    /// <param name="mediaId">The ID of the media for which to purge variants.</param>
    public async Task<Result> PurgeMediaVariantsAsync(Guid mediaId)
    {
        var result = new Result();
        var variants = await cmsContext.MediaVariants
            .Where(v => v.MediaId == mediaId)
            .ToListAsync();

        foreach (var variant in variants)
            await documentService.RemoveDocumentAsync(variant.DocumentId);

        cmsContext.MediaVariants.RemoveRange(variants);
        await cmsContext.SaveChangesAsync();
        return result;
    }

    /// <summary>
    ///     Purges a specific variant by its ID.
    /// </summary>
    /// <param name="variantId">The ID of the variant to purge.</param>
    public async Task<Result> PurgeVariantAsync(Guid variantId)
    {
        var result = new Result();
        var variant = await cmsContext.MediaVariants.FindAsync(variantId);
        if (variant is null)
            return result.AddError(new ResourceNotFoundException());

        await documentService.RemoveDocumentAsync(variant.DocumentId);
        cmsContext.MediaVariants.Remove(variant);
        await cmsContext.SaveChangesAsync();
        return result;
    }

    /// <summary>
    ///     Purges all cached variants across all media.
    /// </summary>
    public async Task<Result> PurgeAllVariantsAsync()
    {
        var result = new Result();
        var variants = await cmsContext.MediaVariants.ToListAsync();

        foreach (var variant in variants)
            await documentService.RemoveDocumentAsync(variant.DocumentId);

        cmsContext.MediaVariants.RemoveRange(variants);
        await cmsContext.SaveChangesAsync();
        return result;
    }

    /// <summary>
    ///     Deletes a media, its original Document, and all variant Documents.
    /// </summary>
    /// <param name="mediaId">The ID of the media to delete.</param>
    public async Task<Result> DeleteMediaAsync(Guid mediaId)
    {
        var result = new Result();
        var media = await cmsContext.Media
            .Include(m => m.Variants)
            .FirstOrDefaultAsync(m => m.Id == mediaId);

        if (media is null)
            return result.AddError(new ResourceNotFoundException());

        foreach (var variant in media.Variants)
            await documentService.RemoveDocumentAsync(variant.DocumentId);

        cmsContext.MediaVariants.RemoveRange(media.Variants);
        cmsContext.Media.Remove(media);
        await cmsContext.SaveChangesAsync();

        await documentService.RemoveDocumentAsync(media.DocumentId);
        return result;
    }

    private async Task<Result<Document>> GenerateVariantAsync(
        Media media,
        Document originalDocument,
        int targetWidth,
        int quality
    )
    {
        var result = new Result<Document>();
        try
        {
            var originalPath = documentService.GetDocumentPath(originalDocument);
            if (!File.Exists(originalPath))
                return result.AddError(new ResourceNotFoundException());

            await using var fileStream = File.OpenRead(originalPath);
            using var image = await Image.LoadAsync(fileStream);

            if (targetWidth >= image.Width)
            {
                result.Value = originalDocument;
                return result;
            }

            var ratio = (double)image.Height / image.Width;
            var targetHeight = (int)(targetWidth * ratio);

            image.Mutate(x => x.Resize(targetWidth, targetHeight));

            var memoryStream = new MemoryStream();
            await image.SaveAsync(memoryStream, new WebpEncoder { Quality = quality });
            var fileBytes = memoryStream.ToArray();

            var variantFileName = $"{Guid.NewGuid()}.webp";
            var document = new Document
            {
                FileName = variantFileName,
                MimeType = "image/webp",
                FileSize = fileBytes.Length
            };

            await digitalContext.Documents.AddAsync(document);

            var storagePath = configuration.Get<string>(CoreSettings.FileSystemPathKey)
                              ?? CoreSettings.DefaultFileSystemPath;
            var fullPath = Path.GetFullPath(Path.Combine(storagePath, variantFileName));
            var dir = Path.GetDirectoryName(fullPath);
            if (dir is not null && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            await File.WriteAllBytesAsync(fullPath, fileBytes);
            await digitalContext.SaveChangesAsync();

            var variant = new MediaVariant
            {
                MediaId = media.Id,
                DocumentId = document.Id,
                Width = targetWidth,
                Height = targetHeight,
                Quality = quality
            };

            await cmsContext.MediaVariants.AddAsync(variant);
            await cmsContext.SaveChangesAsync();

            result.Value = document;
        }
        catch (Exception ex)
        {
            result.AddError(ex);
        }

        return result;
    }
}
