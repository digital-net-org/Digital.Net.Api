namespace Digital.Net.Api.Controllers.Controllers.UserApi.Dto;

public class UserPasswordUpdatePayload
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}