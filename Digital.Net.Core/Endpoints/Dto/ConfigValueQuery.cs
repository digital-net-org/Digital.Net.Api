using Digital.Net.Core.Entities.Models.ConfigValues;
using Digital.Net.Core.Services.Pagination;

namespace Digital.Net.Core.Endpoints.Dto;

public class ConfigValueQuery : Query
{
    public string? Name { get; set; }
    public ConfigValueType? Type { get; set; }
}
