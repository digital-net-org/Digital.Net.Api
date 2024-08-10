using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("recorded_token")]
public class RecordedToken : EntityWithId
{
    [Column("token")]
    [Required]
    [MaxLength(1024)]
    public required string Token { get; init; }

    [ForeignKey("user_id")]
    [Required]
    public virtual required User User { get; set; }

    [Column("ip_address")]
    [Required]
    [MaxLength(45)]
    public required string IpAddress { get; set; }

    [Column("user_agent")]
    [Required]
    [MaxLength(1024)]
    public required string UserAgent { get; set; }

    [Column("expired_at")]
    [Required]
    public required DateTime ExpiredAt { get; set; }
}

