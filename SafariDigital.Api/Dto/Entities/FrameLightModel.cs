using SafariDigital.Data.Models.Frames;

namespace SafariDigital.Api.Dto.Entities;

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