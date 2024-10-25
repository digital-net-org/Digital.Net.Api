using SafariDigital.Data.Models.Database.Frames;
using SafariDigital.Data.Models.Dto.Frames;

namespace Tests.SafariDigital.Database.Models.Dto.Frames;

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
    public void FrameLightModel_DefaultConstructor_ReturnsValidModel()
    {
        var model = new FrameLightModel();
        Assert.NotNull(model);
        Assert.IsType<FrameLightModel>(model);
    }

    [Fact]
    public void FrameModel_ConstructorWithViewFrame_ReturnsValidModel()
    {
        var viewFrame = new Frame
        {
            Id = 1,
            Name = "title",
            Data = "data",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var model = new FrameModel(viewFrame);
        Assert.NotNull(model);
        Assert.IsType<FrameModel>(model);
        Assert.Equal(viewFrame.Id, model.Id);
        Assert.Equal(viewFrame.Name, model.Name);
        Assert.Equal(viewFrame.Data, model.Data);
        Assert.Equal(viewFrame.CreatedAt, model.CreatedAt);
        Assert.Equal(viewFrame.UpdatedAt, model.UpdatedAt);
    }

    [Fact]
    public void FrameLightModel_ConstructorWithViewFrame_ReturnsValidModel()
    {
        var viewFrame = new Frame
        {
            Id = 1,
            Name = "title",
            Data = "data",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        var model = new FrameLightModel(viewFrame);
        Assert.NotNull(model);
        Assert.IsType<FrameLightModel>(model);
        Assert.Equal(viewFrame.Id, model.Id);
        Assert.Equal(viewFrame.Name, model.Name);
        Assert.Equal(viewFrame.CreatedAt, model.CreatedAt);
        Assert.Equal(viewFrame.UpdatedAt, model.UpdatedAt);
    }
}