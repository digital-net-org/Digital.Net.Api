using Digital.Net.Api.Entities.Models.Documents;
using Digital.Net.Api.Entities.Models.PuckConfigs;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;

public class PuckConfigDto
{
    public PuckConfigDto() { }

    public PuckConfigDto(PuckConfig puckConfig)
    {
        Id = puckConfig.Id;
        Version = puckConfig.Version;
        CreatedAt = puckConfig?.CreatedAt;
    }

    public PuckConfigDto(PuckConfig puckConfig, Document document)
    {
        Id = puckConfig.Id;
        Version = puckConfig.Version;
        CreatedAt = puckConfig.CreatedAt;
        UpdatedAt = puckConfig.UpdatedAt;
    }

    public int Id { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}