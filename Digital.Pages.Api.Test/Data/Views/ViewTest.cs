using Digital.Pages.Api.Data.Frames;
using Digital.Pages.Api.Data.Views;

namespace Tests.Dto.Entities;

public class ViewTest
{
    [Fact]
    public void ViewModel_DefaultConstructor_ReturnsValidModel()
    {
        var model = new ViewDto();
        Assert.NotNull(model);
        Assert.IsType<ViewDto>(model);
    }

    [Fact]
    public void ViewModel_ConstructorWithView_ReturnsValidModel()
    {
        var view = new View
        {
            Id = Guid.NewGuid(),
            Title = "title",
            Path = "/",
            IsPublished = true,
            FrameId = Guid.Empty,
            Frame = new Frame
            {
                Id = Guid.Empty,
                Name = "title",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Data = null
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var model = new ViewDto(view);
        Assert.NotNull(model);
        Assert.IsType<ViewDto>(model);
        Assert.Equal(view.Id, model.Id);
        Assert.Equal(view.Title, model.Title);
        Assert.Equal(view.IsPublished, model.IsPublished);
        Assert.Equal(view.FrameId, model.FrameId);
        Assert.Equal(view.CreatedAt, model.CreatedAt);
        Assert.Equal(view.UpdatedAt, model.UpdatedAt);
    }
}