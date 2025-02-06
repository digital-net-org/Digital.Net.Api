using Digital.Net.Core.Random;
using SafariDigital.Data.Models.Avatars;

namespace Tests.Utils.Factories;

public static class AvatarFactoryUtils
{
    public static Avatar CreateAvatar() =>
        new()
        {
            X = Randomizer.GenerateRandomInt(),
            Y = Randomizer.GenerateRandomInt(),
            Document = DocumentFactoryUtils.CreateDocument()
        };
}