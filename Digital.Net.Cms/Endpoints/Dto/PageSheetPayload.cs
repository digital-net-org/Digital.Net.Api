using System.ComponentModel.DataAnnotations;

namespace Digital.Net.Cms.Endpoints.Dto;

public class PageSheetPayload
{
    [Required]
    public required Guid SheetId { get; set; }

    public int LoadOrder { get; set; }
}
