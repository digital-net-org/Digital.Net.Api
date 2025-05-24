using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;
using Digital.Net.Api.Core.Messages;
using Digital.Net.Api.Entities.Models.Users;

namespace Digital.Net.Api.Entities.Models.Events;

[Table("Event")]
public class Event : EntityId
{
    public Event() {}

    public Event SetError(Result result)
    {
        var trace = JsonSerializer.Serialize(result.Errors);
        ErrorTrace = trace.Length > 4096 ? trace[..4096] : trace;
        HasError = true;
        return this;
    }

    [Column("Name"), Required, MaxLength(64)]
    public string Name { get; init; } = string.Empty;

    [Column("Payload"), MaxLength(64)]
    public string? Payload { get; init; } = string.Empty;

    [Column("UserAgent"), Required, MaxLength(1024)]
    public string UserAgent { get; init; } = string.Empty;

    [Column("IpAddress"), Required, MaxLength(45)]
    public string IpAddress { get; init; } = string.Empty;

    [Column("UserId"), ForeignKey("User")]
    public Guid? UserId { get; init; }

    public virtual User? User { get; init; }

    [Column("State")]
    public EventState? State { get; init; }

    [Column("HasError"), Required]
    public bool HasError { get; private set; }

    [Column("ErrorTrace"), MaxLength(4096)]
    public string? ErrorTrace { get; private set; }
}
