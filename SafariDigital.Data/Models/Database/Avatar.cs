using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("avatar")]
public class Avatar : EntityWithId
{
    [ForeignKey("document_id")] [Required] public virtual required Document Document { get; set; }

    [Column("pos_x")] public required int? PosX { get; set; } = null;

    [Column("pos_y")] public required int? PosY { get; set; } = null;
}