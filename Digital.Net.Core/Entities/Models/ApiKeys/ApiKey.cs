using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Random;
using Digital.Net.Lib.String;
using Microsoft.EntityFrameworkCore;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Entities.Models.ApiKeys;

[Table("ApiKey")]
[Index(nameof(Key), IsUnique = true)]
[Index(nameof(UserId), nameof(Name), IsUnique = true)]
public class ApiKey(Guid userId, string name, string? key = null, DateTime? expiredAt = null)
    : Entity, IRestrictedAuditEntity
{
    public static string Hash(string apiKey)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }

    [Column("Name")]
    [MaxLength(64)]
    [Required]
    [RegexValidation(RegularExpressions.ApiKeyNamePattern)]
    public string Name { get; set; } = name;

    [Column("Key")]
    [MaxLength(64)]
    [Required]
    [ReadOnly]
    [Secret]
    public string Key { get; set; } =
        Hash(key ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 128));

    [Column("ExpiredAt")]
    public DateTime? ExpiredAt { get; set; } = expiredAt;

    [Column("UserId")]
    [ForeignKey("User")]
    [Required]
    public Guid UserId { get; set; } = userId;

    public virtual User User { get; set; }
}
