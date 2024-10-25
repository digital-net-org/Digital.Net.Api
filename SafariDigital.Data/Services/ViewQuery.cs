using Safari.Net.Data.Entities;
using SafariDigital.Data.Models.Database;

namespace SafariDigital.Data.Services;

public class ViewQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}