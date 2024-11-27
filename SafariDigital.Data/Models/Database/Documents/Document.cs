using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Data.Models.Database.Documents;

[Table("document"), Index(nameof(FileName), IsUnique = true)]
public class Document : EntityWithGuid
{
    [Column("file_name"), MaxLength(64), Required]
    public required string FileName { get; set; }

    [Column("file_type"), Required]
    public required EDocumentType DocumentType { get; set; }

    [Column("mime_type"), MaxLength(255), Required]
    public required string MimeType { get; set; }

    [Column("file_size"), Required]
    public required long FileSize { get; set; }

    [Column("uploader_id"), ForeignKey("user")]
    public Guid? UploaderId { get; set; }

    public virtual User? Uploader { get; set; }
}

public enum EDocumentType
{
    Avatar
}