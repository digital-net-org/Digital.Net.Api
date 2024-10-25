using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Services.Frames.Models;

public record CreateFrameRequest(
    [Required]
    string Name,
    int? ViewId
);

public record DuplicateFrameRequest(
    [Required]
    string Name
);