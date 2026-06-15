namespace Digital.Net.Cms.Http.Dto;

public class OpenGraphEntryDto
{
    public OpenGraphEntryDto()
    {
    }

    public Guid? Id { get; set; }
    public required string Property { get; set; }
    public required string Content { get; set; }
}