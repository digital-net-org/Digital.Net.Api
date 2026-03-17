using Digital.Net.Cms.Models;

namespace Digital.Net.Cms.Endpoints.Dto;

public class PageSheetDto
{
    public PageSheetDto()
    {
    }

    public PageSheetDto(PageSheet pageSheet)
    {
        SheetId = pageSheet.SheetId;
        Name = pageSheet.Sheet.Name;
        Type = pageSheet.Sheet.Type;
        LoadOrder = pageSheet.LoadOrder;
    }

    public Guid SheetId { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public int LoadOrder { get; set; }
}
