using SafariDigital.Data.Models.Avatars;

namespace SafariDigital.Api.Dto.Entities;

public class AvatarModel
{
    public AvatarModel()
    {
    }

    public AvatarModel(Avatar avatar)
    {
        Id = avatar.Id;
        DocumentId = avatar.DocumentId;
        PosX = avatar.PosX;
        PosY = avatar.PosY;
    }

    public Guid Id { get; init; }
    public Guid? DocumentId { get; init; }
    public int? PosX { get; init; }
    public int? PosY { get; init; }
}