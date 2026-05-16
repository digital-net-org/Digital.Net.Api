using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core.Entities.Models.Documents;

namespace Digital.Net.Cms.Endpoints.Dto;

public class MediaVariantDto
{
    public MediaVariantDto()
    {
    }

    public MediaVariantDto(MediaVariant variant, Document document)
    {
        Id = variant.Id;
        MediaId = variant.MediaId;
        Width = variant.Width;
        Height = variant.Height;
        Quality = variant.Quality;
        FileSize = document.FileSize;
        MimeType = document.MimeType;
        CreatedAt = variant.CreatedAt;
    }

    public Guid Id { get; init; }
    public Guid MediaId { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Quality { get; init; }
    public long FileSize { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public DateTime CreatedAt { get; init; }
}
