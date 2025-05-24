using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Models.Views;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.PuckConfigs;

[Table("PuckConfig"), Index(nameof(Version), IsUnique = true)]
public class PuckConfig : EntityId
{
    [Column("DocumentId"), Required]
    public Guid DocumentId { get; set; }

    [Column("Version"), Required, MaxLength(24)]
    public string Version { get; set; }

    public virtual List<View> Views { get; set; } = [];
}