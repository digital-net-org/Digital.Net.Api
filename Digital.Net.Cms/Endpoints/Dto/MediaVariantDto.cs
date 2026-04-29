using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Medias;

namespace Digital.Net.Cms.Endpoints.Dto;

public class MediaVariantDto
{
    public MediaVariantDto()
    {
    }

    public MediaVariantDto(MediaVariant variant)
    {
        Id = variant.Id;
        MediaId = variant.MediaId;
        Width = variant.Width;
        Height = variant.Height;
        Quality = variant.Quality;
        CreatedAt = variant.CreatedAt;
    }

    public Guid Id { get; init; }
    public Guid MediaId { get; init; }
    public int Width { get; init; }
    public int Height { get; init; }
    public int Quality { get; init; }
    public DateTime CreatedAt { get; init; }
}
