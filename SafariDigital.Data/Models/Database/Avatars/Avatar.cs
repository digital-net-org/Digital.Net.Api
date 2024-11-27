using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Models;
using SafariDigital.Data.Models.Database.Documents;

namespace SafariDigital.Data.Models.Database.Avatars;

[Table("avatar")]
public class Avatar : EntityWithGuid
{
    [Column("pos_x")]
    public int? PosX { get; set; }

    [Column("pos_y")]
    public int? PosY { get; set; }

    [Column("document_id"), ForeignKey("document"), Required]
    public Guid DocumentId { get; set; }

    public virtual Document? Document { get; set; }
}