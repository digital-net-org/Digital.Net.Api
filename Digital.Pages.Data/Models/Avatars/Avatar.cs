using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Lib.Net.Core.Geometry.Models;
using Digital.Lib.Net.Entities.Models;
using Digital.Pages.Data.Models.Documents;

namespace Digital.Pages.Data.Models.Avatars;

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