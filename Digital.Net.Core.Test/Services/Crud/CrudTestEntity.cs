using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Attributes;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Test.Services.Crud;

[Table("TestEntity")]
[Index(nameof(UniqueField), IsUnique = true)]
public class CrudTestEntity : Entity
{
    [Column("Name")]
    [MaxLength(24)]
    [Required]
    [RegexValidation(RegularExpressions.UsernamePattern)]
    public required string Name { get; set; }

    [Column("UniqueField")]
    [MaxLength(64)]
    [Required]
    public required string UniqueField { get; set; }

    [Column("ReadOnlyField")]
    [MaxLength(64)]
    [ReadOnly]
    public string? ReadOnlyField { get; set; }

    public virtual List<CrudTestChild> Children { get; set; } = [];

    [ReadOnly]
    public virtual CrudTestChild? LockedChild { get; set; }
}
