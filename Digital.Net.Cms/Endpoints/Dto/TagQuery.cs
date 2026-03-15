using Digital.Net.Entities.Crud.Endpoints;

namespace Digital.Net.Cms.Endpoints.Dto;

public class TagQuery : Query
{
    public string? Name { get; set; }
}