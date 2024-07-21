using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Services.AuthenticationService.Models;

public record LoginRequest([Required] string Login, [Required] string Password);

public record LoginResponse(string? Token);