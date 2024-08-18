using Safari.Net.Core.Models;
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
        Root = new RootModel { Props = new PropsModel { Title = viewFrame.Title } };
        Content = Mapper.Map<ViewContent, ContentModel>(viewFrame.Content.ToList());
        CreatedAt = viewFrame.CreatedAt;
        UpdatedAt = viewFrame.UpdatedAt;
    }

    public int? Id { get; init; }
    public RootModel? Root { get; set; } = new();
    public List<ContentModel>? Content { get; set; } = [];
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