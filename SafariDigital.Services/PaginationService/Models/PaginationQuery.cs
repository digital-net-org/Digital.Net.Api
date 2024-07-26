namespace SafariDigital.Services.PaginationService.Models;

public class PaginationQuery
{
    public int Index { get; set; } = PaginationUtils.DefaultIndex;
    public int Size { get; set; } = PaginationUtils.DefaultSize;
    public string? OrderBy { get; set; }
}