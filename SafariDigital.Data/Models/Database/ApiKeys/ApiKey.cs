using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database.ApiKeys;

[Table("api_key"), Index(nameof(Key), IsUnique = true)]
public class ApiKey : EntityWithGuid
{
    [Column("key"), MaxLength(128), Required]
    public required string Key { get; set; }

    [Column("expired_at")]
    public DateTime? ExpiredAt { get; set; }
}