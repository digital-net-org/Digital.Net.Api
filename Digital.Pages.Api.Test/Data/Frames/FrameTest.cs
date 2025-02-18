using Digital.Pages.Api.Data.Frames;

namespace Tests.Dto.Entities;

public class FrameTest
{
    [Fact]
    public void FrameModel_DefaultConstructor_ReturnsValidModel()
    {
        var model = new FrameDto();
        Assert.NotNull(model);
        Assert.IsType<FrameDto>(model);
    }

    [Fact]
    public void FrameLightModel_DefaultConstructor_ReturnsValidModel()
    {
        var model = new FrameLightDto();
        Assert.NotNull(model);
        Assert.IsType<FrameLightDto>(model);
    }

    [Fact]
    public void FrameModel_ConstructorWithFrame_ReturnsValidModel()
    {
        var frame = new Frame
        {
            Id = Guid.Empty,
            Name = "title",
            Data = "{\"test\":\"test\"}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var model = new FrameDto(frame);
        Assert.NotNull(model);
        Assert.IsType<FrameDto>(model);
        Assert.Equal(frame.Id, model.Id);
        Assert.Equal(frame.Name, model.Name);
        Assert.Equal(frame.Data, model.Data);
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

        var model = new FrameLightDto(frame);
        Assert.NotNull(model);
        Assert.IsType<FrameLightDto>(model);
        Assert.Equal(frame.Id, model.Id);
        Assert.Equal(frame.Name, model.Name);
        Assert.Equal(frame.CreatedAt, model.CreatedAt);
        Assert.Equal(frame.UpdatedAt, model.UpdatedAt);
    }
}