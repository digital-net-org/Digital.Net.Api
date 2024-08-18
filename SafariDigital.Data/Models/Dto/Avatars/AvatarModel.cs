using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Models.Dto.Avatars;

public class AvatarModel
{
    public AvatarModel()
    {
    }

    public AvatarModel(Avatar avatar)
    {
        Id = avatar.Id;
        documentId = avatar.Document.Id;
        PosX = avatar.PosX;
        PosY = avatar.PosY;
    }

    public int Id { get; init; }
    public Guid documentId { get; init; }
    public int? PosX { get; init; }
    public int? PosY { get; init; }
}