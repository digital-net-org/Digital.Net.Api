using Digital.Net.Core.Messages;

namespace Digital.Net.Entities.Crud.Endpoints;

public class QueryResult<T> : Result<T> where T : class
{
    public int Index { get; set; }
    public int Size { get; set; }
    public int Total { get; set; }
    public new IEnumerable<T> Value { get; set; } = [];
    public int Pages => (int)Math.Ceiling((double)Total / Size);
    public int Count => Value.Count();
    public bool End => Index >= Pages;
}