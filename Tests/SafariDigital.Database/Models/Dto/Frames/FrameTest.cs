using System.Text.Json;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Frames;

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
    public void FrameModel_ConstructorWithFrame_ReturnsValidModel()
    {
        var frame = new Frame
        {
            Id = Guid.Empty,
            Name = "title",
            Data = JsonDocument.Parse("{\"test\":\"test\"}"),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var model = new FrameModel(frame);
        Assert.NotNull(model);
        Assert.IsType<FrameModel>(model);
        Assert.Equal(frame.Id, model.Id);
        Assert.Equal(frame.Name, model.Name);
        Assert.Equal(frame.Data.RootElement, model.Data?.RootElement);
        Assert.Equal(frame.CreatedAt, model.CreatedAt);
        Assert.Equal(frame.UpdatedAt, model.UpdatedAt);
    }

    [Fact]
    public void FrameLightModel_ConstructorWithFrame_ReturnsValidModel()
    {
        var frame = new Frame
        {
            Id = Guid.Empty,
            Name = "title",
            Data = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var model = new FrameLightModel(frame);
        Assert.NotNull(model);
        Assert.IsType<FrameLightModel>(model);
        Assert.Equal(frame.Id, model.Id);
        Assert.Equal(frame.Name, model.Name);
        Assert.Equal(frame.CreatedAt, model.CreatedAt);
        Assert.Equal(frame.UpdatedAt, model.UpdatedAt);
    }
}