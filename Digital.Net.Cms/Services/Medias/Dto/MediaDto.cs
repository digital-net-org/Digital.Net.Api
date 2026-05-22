using Digital.Net.Cms.Models.Medias;
using Digital.Net.Core.Entities.Models.Documents;

namespace Digital.Net.Cms.Services.Medias.Dto;

public class MediaDto
{
    public MediaDto()
    {
    }

    public MediaDto(Media media, Document document)
    {
        Id = media.Id;
        Name = media.Name;
        Alt = media.Alt;
        Published = media.Published;
        DocumentId = media.DocumentId;
        Width = document.Width;
        Height = document.Height;
        FileSize = document.FileSize;
        MimeType = document.MimeType;
        CreatedAt = media.CreatedAt;
        UpdatedAt = media.UpdatedAt;
    }

    public MediaDto(Media media, Document document, IReadOnlyDictionary<Guid, Document> variantDocuments)
        : this(media, document)
    {
        Variants = media.Variants
            .Where(v => variantDocuments.ContainsKey(v.DocumentId))
            .Select(v => new MediaVariantDto(v, variantDocuments[v.DocumentId]))
            .ToList();
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Alt { get; set; }
    public bool Published { get; set; }
    public Guid DocumentId { get; init; }
    public int? Width { get; init; }
    public int? Height { get; init; }
    public long FileSize { get; init; }
    public string MimeType { get; init; } = string.Empty;
    public List<MediaVariantDto> Variants { get; set; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
