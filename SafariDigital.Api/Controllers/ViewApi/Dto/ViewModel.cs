using Digital.Net.Core.Models;
using SafariDigital.Api.Controllers.FrameApi.Dto;
using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Views;

namespace SafariDigital.Api.Controllers.ViewApi.Dto;

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