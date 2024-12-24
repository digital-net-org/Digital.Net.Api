using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Models;
using SafariDigital.Data.Models.Documents;

namespace SafariDigital.Data.Models.Avatars;

[Table("Avatar")]
public class Avatar : EntityGuid
{
    [Column("PosX")]
    public int? PosX { get; set; }

    [Column("PosY")]
    public int? PosY { get; set; }

    [Column("DocumentId"), ForeignKey("Document"), Required]
    public Guid DocumentId { get; set; }

    public virtual Document? Document { get; set; }
}