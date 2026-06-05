namespace Digital.Net.Cms.Http.Dto;

public class PageSheetBuildPayload : PageBuildPayload
{
    public required Guid SheetId { get; set; }
}