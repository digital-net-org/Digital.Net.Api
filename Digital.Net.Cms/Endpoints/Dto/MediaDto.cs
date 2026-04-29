using Digital.Net.Cms.Models;
using Digital.Net.Cms.Models.Medias;

namespace Digital.Net.Cms.Endpoints.Dto;

public class MediaDto
{
    public MediaDto()
    {
    }

    public MediaDto(Media media)
    {
        Id = media.Id;
        Name = media.Name;
        Alt = media.Alt;
        Published = media.Published;
        DocumentId = media.DocumentId;
        CreatedAt = media.CreatedAt;
        UpdatedAt = media.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public bool Published { get; set; }
    public Guid DocumentId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
