using SafariDigital.Data.Models.Database.Frames;

namespace SafariDigital.Api.Controllers.FrameApi.Dto;

public class FrameModel
{
    public FrameModel()
    {
    }

    public FrameModel(Frame frame)
    {
        Id = frame.Id;
        Name = frame.Name;
        Data = frame.GetDecodedData();
        CreatedAt = frame.CreatedAt;
        UpdatedAt = frame.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string Name { get; set; }
    public string? Data { get; set; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}