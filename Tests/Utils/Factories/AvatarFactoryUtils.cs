using Digital.Lib.Net.Core.Random;
using Digital.Pages.Data.Models.Avatars;

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