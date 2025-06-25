namespace Digital.Net.Api.Controllers.Controllers.UserApi.Dto;

public class UserPasswordUpdatePayload
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}