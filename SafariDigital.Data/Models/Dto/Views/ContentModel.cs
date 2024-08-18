using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Models.Dto.Views;

public class ContentModel
{
    public ContentModel()
    {
    }

    public ContentModel(ViewContent content)
    {
        Id = content.Id;
        Type = content.Type;
        Props = content.Props;
        ViewFrameId = content.ViewFrameId;
    }

    public Guid? Id { get; init; }
    public string? Type { get; set; }
    public string? Props { get; set; }
    public int? ViewFrameId { get; set; }
}