using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Cms.Models;

[Table("FormField")]
public class FormField : Entity
{
    [Column("FormId")]
    public Guid FormId { get; set; }

    [Column("Name")]
    [Required]
    [MaxLength(64)]
    public required string Name { get; set; }

    [Column("Type")]
    [Required]
    [MaxLength(16)]
    public required string Type { get; set; }

    [Column("Label")]
    [Required]
    [MaxLength(256)]
    public required string Label { get; set; }

    [Column("Placeholder")]
    [MaxLength(256)]
    public string? Placeholder { get; set; }

    [Column("DefaultValue")]
    [MaxLength(256)]
    public string? DefaultValue { get; set; }

    [Column("Required")]
    public bool Required { get; set; }

    [Column("SortOrder")]
    public int SortOrder { get; set; }

    [Column("ValidationJson")]
    public string? ValidationJson { get; set; }

    [Column("OptionsJson")]
    public string? OptionsJson { get; set; }

    public virtual Form Form { get; set; } = null!;
}
