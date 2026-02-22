using Digital.Net.Api.Core.Interval;

namespace Digital.Net.Api.Entities.Crud.Controllers;

public class Query
{
    public const int DefaultIndex = 1;
    public const int DefaultSize = 50;

    public int Index { get; set; } = DefaultIndex;
    public int Size { get; set; } = DefaultSize;
    public string? OrderBy { get; set; }

    public DateRange? CreatedIn { get; set; }
    public DateRange? UpdatedIn { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void ValidateParameters()
    {
        Index = Index < 1 ? DefaultIndex : Index;
        Size = Size < 1 ? DefaultSize : Size;
    }
}