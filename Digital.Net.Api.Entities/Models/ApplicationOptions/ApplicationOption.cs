using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Entities.Attributes;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.ApplicationOptions;

[Table("ApplicationOption"), Index(nameof(Id), IsUnique = true)]
public class ApplicationOption : Entity
{
    public ApplicationOption() {}

    [Column("Id"), Key, MaxLength(64), Required, ReadOnly]
    public string Id { get; set; }

    [Column("Value"), MaxLength(64), Required]
    public string Value { get; set; }
}