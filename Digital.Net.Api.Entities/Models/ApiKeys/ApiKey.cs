using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Cryptography;
using System.Text;
using Digital.Net.Api.Core.Random;
using Digital.Net.Api.Entities.Attributes;
using Digital.Net.Api.Entities.Models.Users;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Models.ApiKeys;

[Table("ApiKey"), Index(nameof(Key), IsUnique = true)]
public class ApiKey(Guid userId, string? key = null, DateTime? expiredAt = null) : EntityId
{
    public static string Hash(string apiKey)
    {
        var hashBytes = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
        return Convert.ToBase64String(hashBytes);
    }

    [Column("Key"), MaxLength(64), Required, ReadOnly]
    public string Key { get; set; } =
        Hash(key ?? Randomizer.GenerateRandomString(Randomizer.AnyLetterOrNumber, 128));

    [Column("ExpiredAt")]
    public DateTime? ExpiredAt { get; set; } = expiredAt;

    [Column("UserId"), ForeignKey("User"), Required]
    public Guid UserId { get; set; } = userId;

    public virtual User User { get; set; }
}
