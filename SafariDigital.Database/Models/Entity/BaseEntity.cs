using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SafariDigital.Database.Models.Entity;

public class BaseEntity
{
    [Column("created_at")] [Required] public DateTime CreatedAt { get; init; }

    [Column("updated_at")] public DateTime? UpdatedAt { get; init; }
}