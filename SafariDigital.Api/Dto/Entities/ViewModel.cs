using Digital.Lib.Net.Core.Models;
using SafariDigital.Data.Models.Frames;
using SafariDigital.Data.Models.Views;

namespace SafariDigital.Api.Dto.Entities;

public class ViewModel
{
    public ViewModel()
    {
    }

    public ViewModel(View view)
    {
        Id = view.Id;
        Title = view.Title;
        Path = view.Path;
        IsPublished = view.IsPublished;
        FrameId = view.FrameId;
        Frame = view.Frame is not null ? Mapper.MapFromConstructor<Frame, FrameLightModel>(view.Frame) : null;
        CreatedAt = view.CreatedAt;
        UpdatedAt = view.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string? Title { get; set; }
    public string? Path { get; set; }
    public bool? IsPublished { get; set; }
    public Guid? FrameId { get; set; }
    public FrameLightModel? Frame { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}