using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Database.Models.AvatarTable;

namespace SafariDigital.Database.Models.UserTable;

[Table("user")]
[Index(nameof(Username), nameof(Email), IsUnique = true)]
public class User : EntityWithGuid
{
    [Column("username")]
    [MaxLength(24)]
    [Required]
    public required string Username { get; set; }

    [Column("password")]
    [MaxLength(128)]
    [Required]
    public required string Password { get; set; }

    [Column("email")]
    [MaxLength(254)]
    [Required]
    public required string Email { get; set; }

    [Column("role")] [Required] public EUserRole Role { get; set; } = EUserRole.User;

    [ForeignKey("avatar_id")] public virtual Avatar? Avatar { get; set; }
    [Column("is_active")] [Required] public bool IsActive { get; set; } = false;
}