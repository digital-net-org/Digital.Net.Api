namespace Digital.Core.Api.Controllers.UserApi.Dto;

public class UserPasswordUpdatePayload
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}