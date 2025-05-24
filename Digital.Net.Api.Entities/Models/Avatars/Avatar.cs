using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Core.Geometry.Models;
using Digital.Net.Api.Entities.Models.Documents;

namespace Digital.Net.Api.Entities.Models.Avatars;

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
