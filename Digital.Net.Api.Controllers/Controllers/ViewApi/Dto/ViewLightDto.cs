using Digital.Net.Api.Entities.Models.Views;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;

public class ViewLightDto
{
    public ViewLightDto()
    {
    }

    public ViewLightDto(View view)
    {
        Id = view.Id;
        Name = view.Name;
        Version = view.PuckConfig?.Version ?? string.Empty;
        PuckConfigId = view.PuckConfigId;
        CreatedAt = view.CreatedAt;
        UpdatedAt = view.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string Name { get; set; }
    public string Version { get; set; }
    public int PuckConfigId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}