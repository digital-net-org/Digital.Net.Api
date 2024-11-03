using Safari.Net.Data.Entities;

namespace SafariDigital.Data.Models.Database.Views;

public class ViewQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}