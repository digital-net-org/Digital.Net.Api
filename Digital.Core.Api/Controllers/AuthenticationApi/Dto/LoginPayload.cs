using System.ComponentModel.DataAnnotations;

namespace Digital.Core.Api.Controllers.AuthenticationApi.Dto;

public class LoginPayload
{
    [Required]
    public required string Login { get; init; } = string.Empty;

    [Required]
    public required string Password { get; init; } = string.Empty;
}