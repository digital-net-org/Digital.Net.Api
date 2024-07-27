namespace SafariDigital.Services.PaginationService.Models;

public class PaginationResult<T>
{
    public int Index { get; set; }
    public int Size { get; set; }
    public int Total { get; set; }
    public List<T> Result { get; set; } = [];
    public int Pages => (int)Math.Ceiling((double)Total / Size);
    public int Count => Result.Count;
    public bool End => Index >= Pages;
    public bool HasError => Error.Count > 0;
    public List<Exception> Error { get; set; } = [];
    public void AddError(Exception ex) => Error.Add(ex);
}