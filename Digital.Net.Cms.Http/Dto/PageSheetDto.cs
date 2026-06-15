namespace Digital.Net.Cms.Http.Dto;

public class PageSheetDto
{
    public PageSheetDto()
    {
    }

    public Guid? Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required string Content { get; set; }
    public bool Published { get; set; }
}
