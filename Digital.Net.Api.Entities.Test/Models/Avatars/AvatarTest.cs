using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Models.Avatars;
using Digital.Net.Api.Entities.Test.Models.Documents;

namespace Digital.Net.Api.Entities.Test.Models.Avatars;

public class AvatarTest
{
    public static readonly Avatar TestAvatar = new()
    {
        X = Randomizer.GenerateRandomInt(),
        Y = Randomizer.GenerateRandomInt(),
        Document = DocumentTest.TestDocument
    };

    [Fact]
    public void AvatarModel_ReturnsValidModel()
    {
        var dto = Mapper.MapFromConstructor<Avatar, AvatarDto>(TestAvatar);
        Assert.NotNull(dto);
        Assert.IsType<AvatarDto>(dto);
        Assert.Equal(TestAvatar.Id, dto.Id);
        Assert.Equal(TestAvatar.Document?.Id, dto.DocumentId);
        Assert.Equal(TestAvatar.X, dto.X);
        Assert.Equal(TestAvatar.Y, dto.Y);
    }
}