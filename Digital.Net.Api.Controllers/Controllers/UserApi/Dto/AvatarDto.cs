using Digital.Net.Api.Entities.Models.Avatars;

namespace Digital.Net.Api.Controllers.Controllers.UserApi.Dto;

public class AvatarDto
{
    public AvatarDto()
    {
    }

    public AvatarDto(Avatar avatar)
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