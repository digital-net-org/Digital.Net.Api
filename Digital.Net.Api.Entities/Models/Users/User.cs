using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Api.Core.Extensions.StringUtilities;
using Digital.Net.Api.Entities.Attributes;
using Digital.Net.Api.Entities.Models.Avatars;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.Users;

[Table("User"), Index(nameof(Username), nameof(Email), IsUnique = true)]
public class User : EntityGuid
{
    [
        Column("Username"),
        MaxLength(24),
        Required,
        RegexValidation(RegularExpressions.UsernamePattern)
    ]
    public required string Username { get; set; }

    [Column("Email"), MaxLength(254), Required, RegexValidation(RegularExpressions.EmailPattern)]
    public required string Email { get; set; }

    [Column("AvatarId"), ForeignKey("Avatar")]
    public Guid? AvatarId { get; set; }

    public virtual Avatar? Avatar { get; set; }

    [Column("Password"), MaxLength(128), Required, Secret, ReadOnly]
    public required string Password { get; set; }

    [Column("Login"), MaxLength(24), Required]
    public required string Login { get; set; }

    [Column("IsActive"), Required]
    public bool IsActive { get; set; }
}
