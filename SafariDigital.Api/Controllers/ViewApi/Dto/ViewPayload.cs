using System.ComponentModel.DataAnnotations;

namespace SafariDigital.Api.Controllers.ViewApi.Dto;

public class ViewPayload
{
    [Required]
    private string Title { get; set; }
}