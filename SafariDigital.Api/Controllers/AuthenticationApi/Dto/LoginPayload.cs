using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Api.Controllers.AuthenticationApi.Dto;

public class LoginPayload
{
    [Required]
    public string Login { get; set; }

    [Required]
    public string Password { get; set; }
}