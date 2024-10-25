using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Database.Views;
using SafariDigital.Data.Models.Dto.Views;

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
            Id = 1,
            Title = "title",
            IsPublished = true,
            FrameId = 1,
            Frame = new Frame
                {
                    Id = 1,
                    Name = "title",
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Data = "data"
            },
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
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