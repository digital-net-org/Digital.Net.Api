using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Digital.Net.Entities.Models;
using SafariDigital.Data.Models.Database.Users;

namespace SafariDigital.Data.Models.Database.Records;

[Table("recorded_token")]
public class RecordedToken : EntityWithGuid
{
    [Column("token"), Required, MaxLength(1024)]
    public required string Token { get; set; }

    [Column("user_id"), Required, ForeignKey("user")]
    public Guid UserId { get; set; }

    public virtual User User { get; set; }

    [Column("ip_address"), Required, MaxLength(45)]
    public required string IpAddress { get; set; }

    [Column("user_agent"), Required, MaxLength(1024)]
    public required string UserAgent { get; set; }

    [Column("expired_at"), Required]
    public required DateTime ExpiredAt { get; set; }
}