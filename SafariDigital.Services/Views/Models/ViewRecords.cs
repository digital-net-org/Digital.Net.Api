using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Services.Views.Models;

public record CreateViewRequest(
    [Required]
    string Title
);

public record DuplicateViewRequest(
    [Required]
    string Title
);
