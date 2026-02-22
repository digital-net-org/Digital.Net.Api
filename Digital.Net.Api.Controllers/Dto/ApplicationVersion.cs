using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Api.Controllers.Dto;

/// <summary>
///     Represents the Application version details based on the Git repository.
/// </summary>
public record ApplicationVersion(
    [property: Required]
    string Name,
    [property: Required]
    string Origin,
    [property: Required]
    string CommitSha,
    [property: Required]
    string Release
);