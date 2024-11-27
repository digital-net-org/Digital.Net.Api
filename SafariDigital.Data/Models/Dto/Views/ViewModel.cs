using Digital.Net.Core.Models;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Views;
using SafariDigital.Data.Models.Dto.Frames;

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
        FrameId = view.FrameId;
        Frame = view.Frame is not null ? Mapper.MapFromConstructor<Frame, FrameLightModel>(view.Frame) : null;
        CreatedAt = view.CreatedAt;
        UpdatedAt = view.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
    public Guid? FrameId { get; set; }
    public FrameLightModel? Frame { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}