using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models;

[Table("OpenGraphEntry")]
public class OpenGraphEntry : Entity, IUntrackedEntity
{
    [Column("Property")]
    [Required]
    [MaxLength(64)]
    public required string Property { get; set; }

    [Column("Content")]
    [Required]
    [Templatable]
    [MaxLength(2048)]
    public required string Content { get; set; }
}
