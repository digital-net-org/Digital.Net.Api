using Digital.Net.Cms.Models;

namespace Digital.Net.Cms.Endpoints.Dto;

public class SheetDto
{
    public SheetDto()
    {
    }

    public SheetDto(Sheet sheet)
    {
        Id = sheet.Id;
        Name = sheet.Name;
        Type = sheet.Type;
        Content = sheet.Content;
        Published = sheet.Published;
        CreatedAt = sheet.CreatedAt;
        UpdatedAt = sheet.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public bool Published { get; set; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}
