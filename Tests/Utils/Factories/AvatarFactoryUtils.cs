using Digital.Net.Core.Random;
using SafariDigital.Data.Models.Database.Avatars;

namespace Tests.Utils.Factories;

public static class AvatarFactoryUtils
{
    public static Avatar CreateAvatar() =>
        new()
        {
            PosX = Randomizer.GenerateRandomInt(),
            PosY = Randomizer.GenerateRandomInt(),
            Document = DocumentFactoryUtils.CreateDocument()
        };
}