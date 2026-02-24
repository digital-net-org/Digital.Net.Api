using Digital.Net.Api.Entities.Crud.Controllers;

namespace Digital.Net.Api.Controllers.Dto;

public class PageQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
}