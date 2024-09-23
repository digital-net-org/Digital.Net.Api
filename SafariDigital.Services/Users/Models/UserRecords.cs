namespace SafariDigital.Services.Users.Models;

public record UpdatePasswordRequest(string CurrentPassword, string NewPassword);