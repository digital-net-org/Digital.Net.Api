using System.ComponentModel.DataAnnotations;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Services.Views.Models;

public record CreateViewRequest(
    [Required]
    string Title,
    [Required]
    EViewType Type);