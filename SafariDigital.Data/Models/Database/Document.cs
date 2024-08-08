using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("document")]
[Index(nameof(FileName), IsUnique = true)]
public class Document : EntityWithGuid
{
    [Column("file_name")]
    [MaxLength(64)]
    [Required]
    public required string FileName { get; set; }

    [Column("file_type")] [Required] public required EDocumentType DocumentType { get; set; }
    [Column("mime_type")] [Required] public required EMimeType MimeType { get; set; }

    [Column("file_size")] [Required] public required long FileSize { get; set; }

    [ForeignKey("uploader_id")] public virtual User? Uploader { get; set; }
}

public enum EDocumentType
{
    Avatar
}

public enum EMimeType
{
    [Display(Name = "image/png")] Png = 0,
    [Display(Name = "image/jpeg")] Jpg = 1,
    [Display(Name = "image/svg+xml")] Svg = 2,
    [Display(Name = "image/bmp")] Bmp = 3
}