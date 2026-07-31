using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Digital.Net.Core.Entities.Models.Users;
using Digital.Net.Lib.Entities.Attributes;
using Digital.Net.Lib.Entities.Models;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Entities.Models.Sessions;

[Table("Session")]
[Index(nameof(Key), IsUnique = true)]
public class Session(
    Guid userId,
    string key,
    string userAgent,
    DateTime expiredAt,
    DateTime absoluteExpiredAt
) : Entity, IUntrackedEntity
{
    public static string Hash(string sessionId) =>
        Convert.ToBase64String(SHA256.HashData(Encoding.UTF8.GetBytes(sessionId)));

    [Column("Key")]
    [Required]
    [MaxLength(64)]
    [ReadOnly]
    [Secret]
    public string Key { get; init; } = key;

    [Column("UserAgent")]
    [Required]
    [MaxLength(1024)]
    public string UserAgent { get; init; } = userAgent;

    [Column("ExpiredAt")]
    [Required]
    public DateTime ExpiredAt { get; set; } = expiredAt;

    [Column("AbsoluteExpiredAt")]
    [Required]
    public DateTime AbsoluteExpiredAt { get; init; } = absoluteExpiredAt;

    [Column("UserId")]
    [ForeignKey("User")]
    [Required]
    public Guid UserId { get; set; } = userId;

    public virtual User User { get; set; }
}