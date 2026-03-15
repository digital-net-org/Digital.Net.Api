using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.String;
using Digital.Net.Entities.Attributes;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Entities.Test.Models;

[Table("TestEntity")]
[Index(nameof(UniqueField), IsUnique = true)]
public class TestEntity : Entity
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

    public virtual List<TestChild> Children { get; set; } = [];

    [ReadOnly]
    public virtual TestChild? LockedChild { get; set; }
}
