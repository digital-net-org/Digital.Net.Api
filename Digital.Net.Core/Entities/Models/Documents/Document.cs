using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Files;
using Microsoft.EntityFrameworkCore;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Entities.Models.Documents;

[Table("Document")]
[Index(nameof(FileName), IsUnique = true)]
public class Document : Entity
{
    public Document() {}

    public Document(User? uploader, FileDefinition fileDefinition)
    {
        FileName = fileDefinition.GenerateAnonymousFileName();
        MimeType = fileDefinition.MimeType;
        FileSize = fileDefinition.FileSize;
        UploaderId = uploader?.Id;
    }

    [Column("FileName")]
    [MaxLength(64)]
    [Required]
    [ReadOnly]
    public string FileName { get; set; }

    [Column("MimeType")]
    [MaxLength(255)]
    [Required]
    [ReadOnly]
    public string MimeType { get; set; }

    [Column("FileSize")]
    [Required]
    [ReadOnly]
    public long FileSize { get; set; }

    [Column("Width")]
    [ReadOnly]
    public int? Width { get; set; }

    [Column("Height")]
    [ReadOnly]
    public int? Height { get; set; }

    [Column("UploaderId")]
    [ForeignKey("User")]
    public Guid? UploaderId { get; set; }

    public virtual User? Uploader { get; set; }

    public bool IsSvg() => DocumentTypes.SvgMimeTypes.Contains(MimeType, StringComparer.OrdinalIgnoreCase);
}
