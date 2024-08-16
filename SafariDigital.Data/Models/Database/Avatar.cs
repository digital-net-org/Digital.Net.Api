using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("avatar")]
public class Avatar : EntityWithId
{
    [Column("document_id")]
    [ForeignKey("document")]
    [Required]
    public Guid DocumentId { get; set; }

    public virtual Document? Document { get; set; }

    [Column("pos_x")] public int? PosX { get; set; }

    [Column("pos_y")] public int? PosY { get; set; }
}