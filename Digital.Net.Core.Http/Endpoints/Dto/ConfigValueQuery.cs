using Digital.Net.Core.Entities.Models.ConfigValues;
using Digital.Net.Core.Http.Services.Pagination;

namespace Digital.Net.Core.Http.Endpoints.Dto;

public class ConfigValueQuery : Query
{
    public string? Name { get; set; }
    public ConfigValueType? Type { get; set; }
}
