using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SafariDigital.Database.Models.DocumentTable;

namespace SafariDigital.Database.Models.AvatarTable;

[Table("avatar")]
public class Avatar : Entity
{
    [ForeignKey("document_id")] [Required] public virtual required Document Document { get; set; }

    [Column("pos_x")] public required int? PosX { get; set; } = null;

    [Column("pos_y")] public required int? PosY { get; set; } = null;
}