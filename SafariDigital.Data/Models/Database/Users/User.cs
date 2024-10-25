using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;
using SafariDigital.Data.Models.Database.Avatars;

namespace SafariDigital.Data.Models.Database.Users;

[Table("user"), Index(nameof(Username), nameof(Email), IsUnique = true)]
public class User : EntityWithGuid
{
    [Column("username"), MaxLength(24), Required]
    public required string Username { get; set; }

    [Column("password"), Required, MaxLength(128)]
    public required string Password { get; set; }

    [Column("email"), MaxLength(254), Required]
    public required string Email { get; set; }

    [Column("role"), Required]
    public EUserRole Role { get; set; } = EUserRole.User;

    [Column("is_active"), Required]
    public bool IsActive { get; set; } = false;

    [Column("avatar_id"), ForeignKey("avatar")]
    public int? AvatarId { get; set; }

    public virtual Avatar? Avatar { get; set; }
}

