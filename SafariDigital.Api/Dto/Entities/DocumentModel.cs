using SafariDigital.Data.Models.Documents;

namespace SafariDigital.Api.Dto.Entities;

public class DocumentModel
{
    public DocumentModel()
    {
    }

    public DocumentModel(Document document)
    {
        Id = document.Id;
        FileName = document.FileName;
        DocumentType = document.DocumentType;
        MimeType = document.MimeType;
        FileSize = document.FileSize;
        UploaderId = document.Uploader?.Id;
        CreatedAt = document.CreatedAt;
        UpdatedAt = document.UpdatedAt;
    }

    public Guid Id { get; init; }
    public string? FileName { get; init; }
    public DocumentType DocumentType { get; init; }
    public string? MimeType { get; init; }
    public long FileSize { get; init; }
    public Guid? UploaderId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}