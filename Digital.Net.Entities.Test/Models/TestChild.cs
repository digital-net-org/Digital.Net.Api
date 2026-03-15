using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Models;

namespace Digital.Net.Entities.Test.Models;

[Table("TestChild")]
public class TestChild : Entity
{
    [Column("Label")]
    [MaxLength(64)]
    [Required]
    public required string Label { get; set; }

    [Column("TestEntityId")]
    [ForeignKey("Parent")]
    public Guid TestEntityId { get; set; }

    public virtual TestEntity? Parent { get; set; }
}
