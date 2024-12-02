using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Api.Controllers.FrameApi.Dto;

public class FramePayload
{
    [Required]
    public string Name { get; set; }

    public string? Data { get; set; }
}