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
        X = avatar.X;
        Y = avatar.Y;
    }

    public Guid Id { get; init; }
    public Guid? DocumentId { get; init; }
    public int X { get; init; }
    public int Y { get; init; }
}