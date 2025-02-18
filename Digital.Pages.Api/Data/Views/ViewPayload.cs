using System.ComponentModel.DataAnnotations;

namespace Digital.Pages.Api.Data.Views;

public class ViewPayload
{
    [Required]
    public string Title { get; set; }

    [Required]
    public string Path { get; set; }
}