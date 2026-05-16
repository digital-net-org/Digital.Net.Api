namespace Digital.Net.Core.Services.Documents;

public interface IDocumentDimensionExtractor
{
    (int? Width, int? Height) Extract(Stream stream, string mimeType);
}