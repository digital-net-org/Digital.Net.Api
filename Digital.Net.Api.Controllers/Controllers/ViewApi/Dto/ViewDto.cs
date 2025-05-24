using Digital.Net.Api.Entities.Models.Views;

namespace Digital.Net.Api.Controllers.Controllers.ViewApi.Dto;

public class ViewDto : ViewLightDto
{
    public ViewDto()
    {
    }

    public ViewDto(View view)
    {
        Id = view.Id;
        Name = view.Name;
        Data = view.Data;
        PuckConfigId = view.PuckConfigId;
        Version = view.PuckConfig.Version;
        CreatedAt = view.CreatedAt;
        UpdatedAt = view.UpdatedAt;
    }

    public string? Data { get; set; }
}