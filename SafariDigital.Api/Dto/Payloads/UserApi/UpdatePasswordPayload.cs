namespace SafariDigital.Api.Dto.Payloads.UserApi;

public class UpdatePasswordPayload
{
    public string CurrentPassword { get; set; }
    public string NewPassword { get; set; }
}