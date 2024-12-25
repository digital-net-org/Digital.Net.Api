using System.Text.Json;
using SafariDigital.Data.Models.Frames;

namespace SafariDigital.Api.Dto.Entities;

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

    public Guid? Id { get; init; }
    public string Name { get; set; }
    public JsonDocument? Data { get; set; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}