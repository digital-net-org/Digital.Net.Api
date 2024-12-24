using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Frames;
using SafariDigital.Data.Models.Views;

namespace Tests.SafariDigital.Database.Models.Dto.Views;

public class ViewTest
{
    [Fact]
    public void ViewModel_DefaultConstructor_ReturnsValidModel()
    {
        var model = new ViewModel();
        Assert.NotNull(model);
        Assert.IsType<ViewModel>(model);
    }

    [Fact]
    public void ViewModel_ConstructorWithView_ReturnsValidModel()
    {
        var view = new View
        {
            Id = Guid.NewGuid(),
            Title = "title",
            IsPublished = true,
            FrameId = new Guid(),
            Frame = new Frame
            {
                Id = new Guid(),
                Name = "title",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Data = "data"
            },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var model = new ViewModel(view);
        Assert.NotNull(model);
        Assert.IsType<ViewModel>(model);
        Assert.Equal(view.Id, model.Id);
        Assert.Equal(view.Title, model.Title);
        Assert.Equal(view.IsPublished, model.IsPublished);
        Assert.Equal(view.FrameId, model.FrameId);
        Assert.Equal(view.CreatedAt, model.CreatedAt);
        Assert.Equal(view.UpdatedAt, model.UpdatedAt);
    }
}