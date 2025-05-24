using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;

public class ViewPayload
{
    public ViewPayload()
    {
    }

    public ViewPayload(string? data, string name)
    {
        Data = data;
        Name = name;
    }

    [Required]
    public string Name { get; set; }

    [Required]
    public int PuckConfigId { get; set; }

    public string? Data { get; set; }
}