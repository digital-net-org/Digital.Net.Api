using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.Pages;

namespace Digital.Net.Api.Controllers.Controllers.PageApi.Dto;

public class PuckConfigDto
{
    public PuckConfigDto() { }

    public PuckConfigDto(PagePuckConfig pagePuckConfig)
    {
        Id = pagePuckConfig.Id;
        Version = pagePuckConfig.Version;
        CreatedAt = pagePuckConfig?.CreatedAt;
    }

    public PuckConfigDto(PagePuckConfig pagePuckConfig, Document document)
    {
        Id = pagePuckConfig.Id;
        Version = pagePuckConfig.Version;
        CreatedAt = pagePuckConfig.CreatedAt;
        UpdatedAt = pagePuckConfig.UpdatedAt;
    }

    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}