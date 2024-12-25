using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SafariDigital.Api.Dto.Entities;

public class FramePayload
{
    public FramePayload()
    {
    }

    public FramePayload(JsonDocument? data, string name)
    {
        Data = data;
        Name = name;
    }

    [Required]
    public string Name { get; set; }

    public JsonDocument? Data { get; set; }
}