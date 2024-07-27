namespace SafariDigital.Services.UserService.Models;

public record UpdatePasswordRequest(string CurrentPassword, string NewPassword);