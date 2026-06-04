namespace Digital.Net.Lib.Files;

public class FileDefinition
{
    public required string FileName { get; set; }
    public required string MimeType { get; set; }
    public long FileSize { get; set; }

    public string GenerateAnonymousFileName()
    {
        var extension = Path.GetExtension(FileName);
        return $"{Guid.NewGuid()}{extension}";
    }
}