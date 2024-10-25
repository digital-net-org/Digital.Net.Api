using SafariDigital.Data.Models.Database.Frames;

namespace SafariDigital.Data.Models.Dto.Frames;

public class FrameModel
{
    public FrameModel()
    {
    }

    public FrameModel(Frame frame)
    {
        Id = frame.Id;
        Name = frame.Name;
        Data = frame.Data;
        CreatedAt = frame.CreatedAt;
        UpdatedAt = frame.UpdatedAt;
    }

    public int? Id { get; init; }
    public string Name { get; set; }
    public string? Data { get; set; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}