using System.ComponentModel.DataAnnotations;
using Digital.Net.Core.Entities.Models.ConfigValues;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class ConfigValuePayload
{
    [Required]
    public required string Name { get; set; }

    public string? Value { get; set; }

    public ConfigValueType Type { get; set; } = ConfigValueType.String;
}
