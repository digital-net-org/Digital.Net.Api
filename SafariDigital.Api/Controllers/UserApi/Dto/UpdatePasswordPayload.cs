namespace SafariDigital.Api.Controllers.UserApi.Dto;

public class UpdatePasswordPayload
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}