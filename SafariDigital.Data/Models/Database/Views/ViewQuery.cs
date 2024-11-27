using Digital.Net.Entities.Models;

namespace SafariDigital.Data.Models.Database.Views;

public class ViewQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
}