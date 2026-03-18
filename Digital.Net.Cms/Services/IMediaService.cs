using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Lib.Messages;

namespace Digital.Net.Cms.Services;

public interface IMediaService
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
    Task<Result<Document>> GetOrCreateVariantAsync(Guid mediaId, int? width, int? quality);

    /// <summary>
    ///     Purges all cached variants for a given media.
    /// </summary>
    /// <param name="mediaId">The ID of the media for which to purge variants.</param>
    Task<Result> PurgeMediaVariantsAsync(Guid mediaId);

    /// <summary>
    ///     Purges a specific variant by its ID.
    /// </summary>
    /// <param name="variantId">The ID of the variant to purge.</param>   
    Task<Result> PurgeVariantAsync(Guid variantId);

    /// <summary>
    ///     Purges all cached variants across all media.
    /// </summary>
    Task<Result> PurgeAllVariantsAsync();

    /// <summary>
    ///     Deletes a media, its original Document, and all variant Documents.
    /// </summary>
    /// <param name="mediaId">The ID of the media to delete.</param>  
    Task<Result> DeleteMediaAsync(Guid mediaId);
}
