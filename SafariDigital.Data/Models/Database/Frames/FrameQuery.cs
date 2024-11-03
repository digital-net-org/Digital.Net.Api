using Safari.Net.Data.Entities;

namespace SafariDigital.Data.Models.Database.Frames;

public class FrameQuery : Query
{
    public string? Name { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}