namespace Digital.Net.Cms.Services.Pages.Dto;

public class PageSheetBuildPayload : PageBuildPayload
{
    public required Guid SheetId { get; set; }
}