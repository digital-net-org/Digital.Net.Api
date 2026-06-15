using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Digital.Net.Core.Entities.Models.Users;
using Microsoft.EntityFrameworkCore;
using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Entities.Models.ApiTokens;

[Table("ApiToken")]
[Index(nameof(Key), IsUnique = true)]
public class ApiToken : Entity, IUntrackedEntity
{
    public static string Hash(string token)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToBase64String(hashBytes);
    }

    public ApiToken(Guid userId, string key)
    {
        UserId = userId;
        Key = key;
    }

    public ApiToken(Guid userId, string key, string userAgent, DateTime expiresAt)
    {
        UserId = userId;
        Key = key;
        UserAgent = userAgent;
        ExpiredAt = expiresAt;
    }

    [Column("Key")]
    [Required]
    [MaxLength(64)]
    public string Key { get; init; }

    [Column("UserAgent")]
    [Required]
    [MaxLength(1024)]
    public string UserAgent { get; init; } = string.Empty;

    [Column("ExpiredAt")]
    [Required]
    public DateTime? ExpiredAt { get; set; }

    [Column("UserId")]
    [ForeignKey("User")]
    [Required]
    public Guid UserId { get; set; }

    public virtual User User { get; set; }
}
