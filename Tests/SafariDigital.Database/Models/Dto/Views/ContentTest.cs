using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Views;

namespace Tests.SafariDigital.Database.Models.Dto.Views;

public class ContentTest
{
    [Fact]
    public void ContentModel_DefaultConstructor_ReturnsValidModel()
    {
        var model = new ContentModel();
        Assert.NotNull(model);
        Assert.IsType<ContentModel>(model);
    }

    [Fact]
    public void ContentModel_ConstructorWithViewContent_ReturnsValidModel()
    {
        var content = new ViewContent
        {
            Id = Guid.NewGuid(),
            Type = "type",
            Props = "{}",
            ViewFrameId = 1
        };
        var model = new ContentModel(content);
        Assert.NotNull(model);
        Assert.IsType<ContentModel>(model);
        Assert.Equal(content.Id, model.Id);
        Assert.Equal(content.Type, model.Type);
        Assert.Equal(content.Props, model.Props);
        Assert.Equal(content.ViewFrameId, model.ViewFrameId);
    }
}