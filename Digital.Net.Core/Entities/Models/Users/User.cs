using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Core.Entities.Models.ApiKeys;
using Digital.Net.Core.Entities.Models.Avatars;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Entities.Models.Users;

[Table("User")]
[Index(nameof(Username), IsUnique = true)]
[Index(nameof(Email), IsUnique = true)]
[Index(nameof(Login), IsUnique = true)]
public class User : Entity, IRestrictedAuditEntity
{
    [Column("Username")]
    [MaxLength(24)]
    [Required]
    [RegexValidation(RegularExpressions.UsernamePattern)]
    public required string Username { get; set; }

    [Column("Email")]
    [MaxLength(254)]
    [Required]
    [RegexValidation(RegularExpressions.EmailPattern)]
    public required string Email { get; set; }

    [Column("AvatarId")]
    [ForeignKey("Avatar")]
    public Guid? AvatarId { get; set; }

    [ReadOnly]
    public virtual Avatar? Avatar { get; set; }

    [ReadOnly]
    public virtual List<ApiKey> ApiKeys { get; set; } = [];

    [Column("Password")]
    [MaxLength(128)]
    [Required]
    [Secret]
    [ReadOnly]
    public required string Password { get; set; }

    [Column("Login")]
    [MaxLength(24)]
    [Required]
    [ReadOnly]
    [RegexValidation(RegularExpressions.UsernamePattern)]
    public required string Login { get; set; }

    [Column("IsActive")]
    [Required]
    [ReadOnly]
    public bool IsActive { get; set; }

    [Column("IsAdmin")]
    [Required]
    [ReadOnly]
    public bool IsAdmin { get; set; }
}
