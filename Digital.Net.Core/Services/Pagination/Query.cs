namespace Digital.Net.Core.Services.Pagination;

public class Query
{
    public const int DefaultIndex = 1;
    public const int DefaultSize = 50;

    public int Index { get; set; } = DefaultIndex;
    public int Size { get; set; } = DefaultSize;
    public string? OrderBy { get; set; }
    public string? Order { get; set; }
    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
    public DateTime? UpdatedFrom { get; set; }
    public DateTime? UpdatedTo { get; set; }

    public void ValidateParameters()
    {
        Index = Index < 1 ? DefaultIndex : Index;
        Size = Size < 1 ? DefaultSize : Size;
    }
}