using Digital.Net.Entities.Crud.Enpoints;

namespace Digital.Net.Api.Endpoints.Dto;

public class PageQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
}