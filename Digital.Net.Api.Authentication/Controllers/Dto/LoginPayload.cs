using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Authentication.Controllers.Dto;

/// <summary>
///     Represents the payload required for user login authentication.
/// </summary>
public record LoginPayload(
    [property: Required]
    string Login,
    [property: Required]
    string Password
);