using Safari.Net.Data.Entities;

namespace SafariDigital.Data.Services;

public class ViewFrameQuery : Query
{
    public string? Name { get; set; }
    public int? ViewId { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}