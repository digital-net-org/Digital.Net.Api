using SafariDigital.Database.Models.AvatarTable;
using Tests.Core.Utils;

namespace Tests.Core.Factories;

public static class AvatarFactory
{
    public static Avatar CreateAvatar() =>
        new()
        {
            PosX = RandomUtils.GenerateRandomInt(),
            PosY = RandomUtils.GenerateRandomInt(),
            Document = DocumentFactory.CreateDocument()
        };
}