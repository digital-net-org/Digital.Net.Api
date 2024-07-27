using SafariDigital.Database.Models.DocumentTable;

namespace SafariDigital.DataIdentities.Models.Document;

public class DocumentModel
{
    public DocumentModel()
    {
    }

    public DocumentModel(Database.Models.DocumentTable.Document document)
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
    public EDocumentType DocumentType { get; init; }
    public EMimeType MimeType { get; init; }
    public long FileSize { get; init; }
    public Guid? UploaderId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}