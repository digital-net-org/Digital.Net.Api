using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Microsoft.EntityFrameworkCore;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Entities.Models.Auth;

[Table("AuthEvent")]
[Index(nameof(IpAddress), nameof(Type), nameof(Success), nameof(CreatedAt))]
[Index(nameof(UserId), nameof(CreatedAt))]
[Index(nameof(CreatedAt), nameof(Id))]
public class AuthEvent : Entity, IUntrackedEntity
{
    [Column("Type")]
    [Required]
    [Sortable]
    public AuthEventType Type { get; init; }

    [Column("Success")]
    [Required]
    [Sortable]
    public bool Success { get; init; }

    [Column("Login")]
    [MaxLength(24)]
    [Sortable]
    public string? Login { get; init; }

    [Column("UserId")]
    public Guid? UserId { get; init; }

    [Column("IpAddress")]
    [MaxLength(45)]
    [Sortable]
    public string? IpAddress { get; init; }

    [Column("UserAgent")]
    [MaxLength(1024)]
    public string? UserAgent { get; init; }
}