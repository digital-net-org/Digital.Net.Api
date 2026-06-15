using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Entities.Models.ConfigValues;

[Table("ConfigValue")]
[Index(nameof(Name), IsUnique = true)]
public class ConfigValue : Entity
{
    [Column("Name")]
    [MaxLength(128)]
    [Required]
    [RegexValidation(RegularExpressions.ConfigValueNamePattern)]
    public required string Name { get; set; }

    [Column("Value")]
    [MaxLength(1048576)]
    public string? Value { get; set; }

    [Column("Type")]
    [MaxLength(16)]
    [Required]
    public ConfigValueType Type { get; set; } = ConfigValueType.String;
}
