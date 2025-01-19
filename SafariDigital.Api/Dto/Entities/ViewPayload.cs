using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Api.Dto.Entities;

public class ViewPayload
{
    [Required]
    public string Title { get; set; }
}