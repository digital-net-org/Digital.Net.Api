using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Core.Http.Endpoints.Dto;

/// <summary>
///     Represents the Application version details based on the Git repository.
/// </summary>
public record ApplicationVersion(
    [property: Required]
    string Application,
    [property: Required]
    string Framework,
    [property: Required]
    string Origin,
    [property: Required]
    string CommitSha,
    [property: Required]
    string Release
);