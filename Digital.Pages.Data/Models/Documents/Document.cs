using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Entities.Attributes;
using Digital.Lib.Net.Entities.Models;
using Digital.Pages.Data.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Digital.Pages.Data.Models.Documents;

[Table("Document"), Index(nameof(FileName), IsUnique = true)]
public class Document : EntityGuid
{
    [Column("FileName"), MaxLength(64), Required, ReadOnly]
    public required string FileName { get; set; }

    [Column("MimeType"), MaxLength(255), Required, ReadOnly]
    public required string MimeType { get; set; }

    [Column("FileSize"), Required, ReadOnly]
    public required long FileSize { get; set; }

    [Column("UploaderId"), ForeignKey("User")]
    public Guid? UploaderId { get; set; }

    public virtual User? Uploader { get; set; }
}

