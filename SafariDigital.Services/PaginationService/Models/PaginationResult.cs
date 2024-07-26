namespace SafariDigital.Services.PaginationService.Models;

public class PaginationResult<T>
{
    public int Index { get; init; }
    public int Size { get; init; }
    public int Total { get; init; }
    public List<T> Items { get; init; } = [];
    public int Pages => (int)Math.Ceiling((double)Total / Size);
    public int Count => Items.Count;
    public bool End => Index >= Pages;
}