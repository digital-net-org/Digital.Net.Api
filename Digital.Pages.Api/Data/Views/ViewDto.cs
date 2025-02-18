using Digital.Lib.Net.Core.Models;
using Digital.Pages.Api.Data.Frames;

namespace Digital.Pages.Api.Data.Views;

public class ViewDto
{
    public ViewDto()
    {
    }

    public ViewDto(View view)
    {
        Id = view.Id;
        Title = view.Title;
        Path = view.Path;
        IsPublished = view.IsPublished;
        FrameId = view.FrameId;
        Frame = view.Frame is not null ? Mapper.MapFromConstructor<Frame, FrameLightDto>(view.Frame) : null;
        CreatedAt = view.CreatedAt;
        UpdatedAt = view.UpdatedAt;
    }

    public Guid? Id { get; init; }
    public string? Title { get; set; }
    public string? Path { get; set; }
    public bool? IsPublished { get; set; }
    public Guid? FrameId { get; set; }
    public FrameLightDto? Frame { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}