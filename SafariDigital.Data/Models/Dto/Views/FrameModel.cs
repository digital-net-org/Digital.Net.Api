using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Models.Dto.Views;

public class FrameModel
{
    public FrameModel()
    {
    }

    public FrameModel(ViewFrame viewFrame)
    {
        Id = viewFrame.Id;
        Name = viewFrame.Name;
        Data = viewFrame.Data;
        CreatedAt = viewFrame.CreatedAt;
        UpdatedAt = viewFrame.UpdatedAt;
    }

    public int? Id { get; init; }
    public string Name { get; set; }
    public string? Data { get; set; }
    public DateTime? CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public class RootModel
{
    public PropsModel Props { get; set; } = new();
}

public class PropsModel
{
    public string? Title { get; set; }
}