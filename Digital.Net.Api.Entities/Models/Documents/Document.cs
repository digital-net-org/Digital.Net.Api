using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Attributes;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.Documents;

[Table("Document"), Index(nameof(FileName), IsUnique = true)]
public class Document : Entity
{
    public Document() {}

    public Document(User? uploader, IFormFile file)
    {
        FileName = GenerateAnonymousFileName(file.FileName);
        MimeType = file.ContentType;
        FileSize = file.Length;
        UploaderId = uploader?.Id;
    }

    [Column("FileName"), MaxLength(64), Required, ReadOnly]
    public string FileName { get; set; }

    [Column("MimeType"), MaxLength(255), Required, ReadOnly]
    public string MimeType { get; set; }

    [Column("FileSize"), Required, ReadOnly]
    public long FileSize { get; set; }

    [Column("UploaderId"), ForeignKey("User")]
    public Guid? UploaderId { get; set; }

    public virtual User? Uploader { get; set; }

    private static string GenerateAnonymousFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        return $"{Guid.NewGuid()}{extension}";
    }
}
