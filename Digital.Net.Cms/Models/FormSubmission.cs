using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models;

[Table("FormSubmission")]
public class FormSubmission : Entity
{
    [Column("FormId")]
    public Guid FormId { get; set; }

    [Column("ValuesJson")]
    [Required]
    public required string ValuesJson { get; set; }

    [Column("SubmitterIp")]
    [Required]
    [MaxLength(64)]
    public required string SubmitterIp { get; set; }

    [Column("UserAgent")]
    [Required]
    [MaxLength(512)]
    public required string UserAgent { get; set; }

    public virtual Form Form { get; set; } = null!;
}
