using SafariDigital.DataIdentities.Models.Avatar;
using Tests.Core.Factories;

namespace Tests.Unit.SafariDigital.Database.Models.AvatarTable;

public class AvatarTest
{
    [Fact]
    public void GetModel_ReturnsCorrectModelType()
    {
        // Arrange
        var avatar = AvatarFactory.CreateAvatar();

        // Act
        var avatarModel = avatar.GetModel<AvatarModel>();

        // Assert
        Assert.NotNull(avatarModel);
        Assert.IsType<AvatarModel>(avatarModel);
        Assert.Equal(avatar.Id, avatarModel.Id);
        Assert.Equal(avatar.Document.Id, avatarModel.documentId);
        Assert.Equal(avatar.PosX, avatarModel.PosX);
        Assert.Equal(avatar.PosY, avatarModel.PosY);
    }
}