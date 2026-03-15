namespace Digital.Net.Core.Endpoints.Dto;

public class UserPasswordUpdatePayload
{
    public required string CurrentPassword { get; set; }
    public required string NewPassword { get; set; }
}