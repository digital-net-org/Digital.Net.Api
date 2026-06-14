using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Models.Auth;

[Table("AuthEvent")]
[Index(nameof(IpAddress), nameof(Type), nameof(Success), nameof(CreatedAt))]
[Index(nameof(UserId), nameof(CreatedAt))]
[Index(nameof(CreatedAt), nameof(Id))]
public class AuthEvent : Entity, IUntrackedEntity
{
    [Column("Type")]
    [Required]
    public AuthEventType Type { get; init; }

    [Column("Success")]
    [Required]
    public bool Success { get; init; }

    [Column("Login")]
    [MaxLength(24)]
    public string? Login { get; init; }

    [Column("UserId")]
    public Guid? UserId { get; init; }

    [Column("IpAddress")]
    [MaxLength(45)]
    public string? IpAddress { get; init; }

    [Column("UserAgent")]
    [MaxLength(1024)]
    public string? UserAgent { get; init; }
}