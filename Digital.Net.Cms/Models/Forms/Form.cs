using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models.Forms;

[Table("Form")]
public class Form : Entity
{
    [Column("Name")]
    [Required]
    [MaxLength(256)]
    public required string Name { get; set; }

    [Column("Description")]
    [MaxLength(512)]
    public string? Description { get; set; }

    [Column("Published")]
    public bool Published { get; set; }

    [Column("SubmitLabel")]
    [MaxLength(128)]
    public string SubmitLabel { get; set; } = "Submit";

    public virtual List<FormField> Fields { get; set; } = [];
    public virtual List<FormSubmission> Submissions { get; set; } = [];
}
