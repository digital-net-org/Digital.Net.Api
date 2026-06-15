using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Cms.Models.Forms;

[Table("FormSubmission")]
public class FormSubmission : Entity
{
    [Column("FormId")]
    public Guid FormId { get; set; }

    [Column("ValuesJson")]
    [Required]
    public required string ValuesJson { get; set; }

    [Column("SubmitterIp")]
    [MaxLength(64)]
    public string? SubmitterIp { get; set; }

    [Column("UserAgent")]
    [MaxLength(512)]
    public string? UserAgent { get; set; }

    public virtual Form Form { get; set; } = null!;
}
