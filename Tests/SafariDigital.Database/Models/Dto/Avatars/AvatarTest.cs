using Digital.Net.Core.Models;
using SafariDigital.Api.Dto.Entities;
using SafariDigital.Data.Models.Avatars;
using Tests.Utils.Factories;

namespace Tests.SafariDigital.Database.Models.Dto.Avatars;

public class AvatarTest
{
    [Fact]
    public void AvatarModel_ReturnsValidModel()
    {
        var avatar = AvatarFactoryUtils.CreateAvatar();
        var avatarModel = Mapper.MapFromConstructor<Avatar, AvatarModel>(avatar);
        Assert.NotNull(avatarModel);
        Assert.IsType<AvatarModel>(avatarModel);
        Assert.Equal(avatar.Id, avatarModel.Id);
        Assert.Equal(avatar.Document?.Id, avatarModel.DocumentId);
        Assert.Equal(avatar.X, avatarModel.X);
        Assert.Equal(avatar.Y, avatarModel.Y);
    }
}