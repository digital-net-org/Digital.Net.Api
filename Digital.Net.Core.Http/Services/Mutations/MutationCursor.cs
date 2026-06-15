using Digital.Net.Lib.Entities.Mutations;

namespace Digital.Net.Core.Http.Services.Mutations;

/// <summary>
///     Cross-schema order over mutations: <c>(CreatedAt, Id)</c>. Serialized as the SSE
///     <c>Last-Event-Id</c> / <c>id:</c> field so a reconnecting client can resume exactly where it stopped.
/// </summary>
public readonly record struct MutationCursor(DateTime CreatedAt, Guid Id)
{
    public static MutationCursor From(MutationSignal signal) => new(signal.CreatedAt, signal.Id);

    /// <summary>
    ///     parseable encoding: <c>{ticks}:{guid:N}</c> (ticks of the UTC timestamp).
    /// </summary>
    public string Format() => $"{CreatedAt.Ticks}:{Id:N}";

    public static MutationCursor? TryParse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var parts = value.Split(':', 2);
        if (parts.Length != 2
            || !long.TryParse(parts[0], out var ticks)
            || ticks < DateTime.MinValue.Ticks || ticks > DateTime.MaxValue.Ticks
            || !Guid.TryParseExact(parts[1], "N", out var id))
            return null;
        return new MutationCursor(new DateTime(ticks, DateTimeKind.Utc), id);
    }
}