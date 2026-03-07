using Digital.Net.Entities.Crud.Controllers;

namespace Digital.Net.Controllers.Dto;

public class PageQuery : Query
{
    public string? Title { get; set; }
    public bool? IsPublished { get; set; }
}