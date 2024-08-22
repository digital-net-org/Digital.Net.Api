using System.ComponentModel.DataAnnotations;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Services.Views.Models;

public record CreateViewRequest(
    [Required]
    string Title,
    [Required]
    EViewType Type);

public record CreateViewFrameRequest(
    [Required]
    string Name);

public record DuplicateViewRequest(
    [Required]
    string Title);

public record DuplicateViewFrameRequest(
    [Required]
    string Name);