using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Attributes;
using Digital.Net.Entities.Models;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Data.Models.Database.Avatars;

namespace SafariDigital.Data.Models.Database.Users;

[Table("user"), Index(nameof(Username), nameof(Email), IsUnique = true)]
public class User : EntityWithGuid
{
    [Column("username"), MaxLength(24), Required, RegexValidation("^[a-zA-Z0-9.'@_-]{6,24}$")]
    public required string Username { get; set; }

    [Column("password"), MaxLength(128), Required, Secret]
    public required string Password { get; set; }

    [Column("email"), MaxLength(254), Required, RegexValidation(@"^[^@]+@[^@]+\.[^@]{2,253}$")]
    public required string Email { get; set; }

    [Column("role"), Required]
    public EUserRole Role { get; set; } = EUserRole.User;

    [Column("is_active"), Required]
    public bool IsActive { get; set; } = false;

    [Column("avatar_id"), ForeignKey("avatar")]
    public Guid? AvatarId { get; set; }

    public virtual Avatar? Avatar { get; set; }
}

