using Digital.Net.Core.Entities.Context;
using Digital.Net.Core.Entities.Models.Mutations;
using Digital.Net.Core.Entities.Mutations;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Digital.Net.Core.Http.Services.Mutations;

public class MutationCatchupReader(DigitalContext context, IEnumerable<MutationSchema> schemas)
{
    private const int MaxCatchUp = 1000;

    public async Task<IReadOnlyList<MutationSignal>> ReadSinceAsync(
        MutationCursor? cursor,
        IReadOnlySet<string>? entityTypes,
        CancellationToken cancellationToken
    )
    {
        // No cursor ⇒ first connection: the client just loaded its data and has nothing cached to replay.
        if (cursor is null) return [];

        var filters = new List<string>();
        var parameters = new List<object>();
        if (cursor is { } c)
        {
            filters.Add("(\"CreatedAt\" > @ts OR (\"CreatedAt\" = @ts AND \"Id\" > @id))");
            parameters.Add(new NpgsqlParameter("ts", NpgsqlDbType.TimestampTz) { Value = c.CreatedAt });
            parameters.Add(new NpgsqlParameter("id", NpgsqlDbType.Uuid) { Value = c.Id });
        }

        // Applied even when empty (= ANY('{}') matches nothing): an empty visibility whitelist must
        // yield an empty catch-up, never an unfiltered one.
        if (entityTypes is not null)
        {
            filters.Add("\"EntityType\" = ANY(@types)");
            parameters.Add(new NpgsqlParameter("types", NpgsqlDbType.Array | NpgsqlDbType.Text)
                { Value = entityTypes.ToArray() });
        }

        var schemaNames = schemas.Select(s => s.Name).Distinct().ToList();
        if (schemaNames.Count == 0) return [];

        var where = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : string.Empty;
        const string columns = "\"ChangeType\", \"EntityType\", \"EntityId\", \"CreatedAt\", \"Id\", \"UserId\"";
        var union = string.Join(
            "\nUNION ALL\n",
            schemaNames.Select(schema => $"SELECT {columns} FROM {schema}.\"EntityMutation\" {where}")
        );
        var sql = $"{union}\nORDER BY \"CreatedAt\", \"Id\"\nLIMIT {MaxCatchUp}";

        var rows = await context.Database
            .SqlQueryRaw<MutationRow>(sql, parameters.ToArray())
            .ToListAsync(cancellationToken);

        return rows
            .Select(r => new MutationSignal((ChangeType)r.ChangeType, r.EntityType, r.EntityId, r.CreatedAt, r.Id, r.UserId))
            .ToList();
    }
}