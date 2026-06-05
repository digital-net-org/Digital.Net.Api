using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Core.Entities.Models;

namespace Digital.Net.Tests.Core.Http.Services.Crud;

[Table("TestChild")]
public class CrudTestChild : Entity
{
    [Column("Label")]
    [MaxLength(64)]
    [Required]
    public required string Label { get; set; }

    [Column("TestEntityId")]
    [ForeignKey("Parent")]
    public Guid TestEntityId { get; set; }

    public virtual CrudTestEntity? Parent { get; set; }
}
