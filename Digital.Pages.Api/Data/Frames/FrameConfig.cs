using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Entities.Models;
using Digital.Lib.Net.Entities.Models.Documents;
using Microsoft.EntityFrameworkCore;

namespace Digital.Pages.Api.Data.Frames;

[Table("FrameConfig"), Index(nameof(Version), IsUnique = true)]
public class FrameConfig : EntityId
{
    [Column("Version"), Required, MaxLength(24)]
    public required string Version { get; set; }

    [Column("IsDefault"), Required]
    public required bool IsDefault { get; set; }

    [Column("DocumentId"), ForeignKey("Document"), Required]
    public required Guid DocumentId { get; set; }

    public virtual required Document Document { get; set; }
}