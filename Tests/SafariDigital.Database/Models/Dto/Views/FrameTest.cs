using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Views;

namespace Tests.SafariDigital.Database.Models.Dto.Views;

public class FrameTest
{
    [Fact]
    public void FrameModel_DefaultConstructor_ReturnsValidModel()
    {
        var model = new FrameModel();
        Assert.NotNull(model);
        Assert.IsType<FrameModel>(model);
    }

    [Fact]
    public void FrameModel_ConstructorWithViewFrame_ReturnsValidModel()
    {
        var viewFrame = new ViewFrame
        {
            Id = 1,
            Title = "title",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now,
            Content = new List<ViewContent>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Type = "type",
                    Props = "{}",
                    ViewFrameId = 1
                }
            }
        };

        var model = new FrameModel(viewFrame);
        Assert.NotNull(model);
        Assert.IsType<FrameModel>(model);
        Assert.Equal(viewFrame.Id, model.Id);
        Assert.Equal(viewFrame.Title, model.Root!.Props.Title);
        Assert.Equal(viewFrame.CreatedAt, model.CreatedAt);
        Assert.Equal(viewFrame.UpdatedAt, model.UpdatedAt);
    }
}