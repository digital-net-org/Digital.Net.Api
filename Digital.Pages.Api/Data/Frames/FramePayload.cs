using System.ComponentModel.DataAnnotations;

namespace Digital.Pages.Api.Data.Frames;

public class FramePayload
{
    public FramePayload()
    {
    }

    public FramePayload(string? data, string name)
    {
        Data = data;
        Name = name;
    }

    [Required]
    public string Name { get; set; }

    public string? Data { get; set; }
}