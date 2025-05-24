using Digital.Net.Api.Core.Interval;

namespace Digital.Net.Api.Controllers.Generic.Pagination;

public class Query
{
    public int Index { get; set; } = PaginationUtils.DefaultIndex;
    public int Size { get; set; } = PaginationUtils.DefaultSize;
    public string? OrderBy { get; set; }

    public DateRange? CreatedIn { get; set; }
    public DateRange? UpdatedIn { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}