using SafariDigital.Data.Models.Database.Frames;

namespace SafariDigital.Api.Controllers.FrameApi.Dto;

public class FrameLightModel
{
    public FrameLightModel()
    {
    }

    public FrameLightModel(Frame frame)
    {
        Id = frame.Id;
        Name = frame.Name;
        CreatedAt = frame.CreatedAt;
        UpdatedAt = frame.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string Name { get; set; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}