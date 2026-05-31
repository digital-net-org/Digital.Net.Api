using Digital.Net.Core.Entities.Models.ConfigValues;

namespace Digital.Net.Core.Endpoints.Dto;

public class ConfigValueDto
{
    public ConfigValueDto()
    {
    }

    public ConfigValueDto(ConfigValue configValue)
    {
        Id = configValue.Id;
        Name = configValue.Name;
        Value = configValue.Value;
        Type = configValue.Type;
        CreatedAt = configValue.CreatedAt;
        UpdatedAt = configValue.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string? Value { get; set; }
    public ConfigValueType Type { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
