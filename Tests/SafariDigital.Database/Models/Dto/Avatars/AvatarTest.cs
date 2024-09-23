using Safari.Net.Core.Models;
using SafariDigital.Data.Models.Database;
using SafariDigital.Data.Models.Dto.Avatars;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Database.Models.Dto.Avatars;

public class AvatarTest
{
    [Fact]
    public void AvatarModel_ReturnsValidModel()
    {
        var avatar = AvatarFactoryUtils.CreateAvatar();
        var avatarModel = Mapper.Map<Avatar, AvatarModel>(avatar);
        Assert.NotNull(avatarModel);
        Assert.IsType<AvatarModel>(avatarModel);
        Assert.Equal(avatar.Id, avatarModel.Id);
        Assert.Equal(avatar.Document.Id, avatarModel.DocumentId);
        Assert.Equal(avatar.PosX, avatarModel.PosX);
        Assert.Equal(avatar.PosY, avatarModel.PosY);
    }
}