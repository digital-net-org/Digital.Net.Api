using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Lib.Messages;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Digital.Net.Cms.Services;

public interface IMediaService
{
    /// <summary>
    ///     Returns the Document to serve for a media image request.
    ///     If width/quality are provided and the media is a raster image,
    ///     returns an existing or newly generated variant Document.
    ///     For SVGs or requests without resize params, returns the original Document.
    /// </summary>
    Task<Result<Document>> GetOrCreateVariantAsync(Guid mediaId, int? width, int? quality);

    /// <summary>
    ///     Purges all cached variants for a given media.
    /// </summary>
    Task<Result> PurgeMediaVariantsAsync(Guid mediaId);

    /// <summary>
    ///     Purges a specific variant by its ID.
    /// </summary>
    Task<Result> PurgeVariantAsync(Guid variantId);

    /// <summary>
    ///     Purges all cached variants across all media.
    /// </summary>
    Task<Result> PurgeAllVariantsAsync();

    /// <summary>
    ///     Deletes a media, its original Document, and all variant Documents.
    /// </summary>
    Task<Result> DeleteMediaAsync(Guid mediaId);
}
