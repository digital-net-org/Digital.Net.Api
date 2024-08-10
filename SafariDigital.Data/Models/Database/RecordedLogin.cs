using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Safari.Net.Data.Entities.Models;

namespace SafariDigital.Data.Models.Database;

[Table("recorded_login")]
public class RecordedLogin : EntityWithId
{
    [Column("ip_address")]
    [Required]
    [MaxLength(45)]
    public required string IpAddress { get; set; }

    [Column("user_agent")]
    [Required]
    [MaxLength(1024)]
    public required string UserAgent { get; set; }

    [Column("success")]
    public bool Success { get; init; }
}

