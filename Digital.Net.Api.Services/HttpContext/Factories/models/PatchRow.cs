namespace Digital.Net.Api.Services.HttpContext.Factories.models;

public class PatchRow(string op, string path, object value)
{
    public string Op { get; set; } = op;
    public string Path { get; set; } = path;
    public object Value { get; set; } = value;
}