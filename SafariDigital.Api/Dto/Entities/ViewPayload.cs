using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Api.Dto.Entities;

public class ViewPayload
{
    [Required]
    private string Title { get; set; }
}