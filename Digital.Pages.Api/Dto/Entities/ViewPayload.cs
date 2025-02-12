using System.ComponentModel.DataAnnotations;

namespace Digital.Pages.Api.Dto.Entities;

public class ViewPayload
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Path { get; set; }
}