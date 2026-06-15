using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Models.Documents;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Entities.Models.Avatars;

[Table("Avatar")]
public class Avatar : Entity, IUntrackedEntity
{
    [Column("X")]
    public int X { get; set; }

    [Column("Y")]
    public int Y { get; set; }

    [Column("DocumentId")]
    [ForeignKey("Document")]
    [Required]
    public Guid DocumentId { get; set; }

    public virtual Document? Document { get; set; }
}
