using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Models.Dto.Views;

public class FrameLightModel
{
    public FrameLightModel()
    {
    }

    public FrameLightModel(ViewFrame viewFrame)
    {
        Id = viewFrame.Id;
        Name = viewFrame.Name;
        CreatedAt = viewFrame.CreatedAt;
        UpdatedAt = viewFrame.UpdatedAt;
    }

    public int? Id { get; init; }
    public string Name { get; set; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}