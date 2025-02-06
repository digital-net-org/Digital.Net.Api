using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Geometry.Models;
using Digital.Net.Entities.Models;
using SafariDigital.Data.Models.Documents;

namespace SafariDigital.Data.Models.Avatars;

[Table("Avatar")]
public class Avatar : EntityGuid, IPosition
{
    [Column("X")]
    public int X { get; set; }

    [Column("Y")]
    public int Y { get; set; }

    [Column("DocumentId"), ForeignKey("Document"), Required]
    public Guid DocumentId { get; set; }

    public virtual Document? Document { get; set; }
}