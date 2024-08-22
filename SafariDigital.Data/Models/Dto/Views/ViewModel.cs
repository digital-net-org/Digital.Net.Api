using Safari.Net.Core.Models;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Models.Dto.Views;

public class ViewModel
{
    public ViewModel()
    {
    }

    public ViewModel(View view)
    {
        Id = view.Id;
        Title = view.Title;
        IsPublished = view.IsPublished;
        Type = view.Type;
        PublishedFrameId = view.PublishedFrameId;
        Frames = Mapper.Map<ViewFrame, FrameLightModel>(view.Frames.ToList());
        CreatedAt = view.CreatedAt;
        UpdatedAt = view.UpdatedAt;
    }

    public int? Id { get; init; }
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
    public EViewType? Type { get; set; }
    public int? PublishedFrameId { get; set; }
    public List<FrameLightModel>? Frames { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}