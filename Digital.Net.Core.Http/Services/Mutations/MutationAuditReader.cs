using Digital.Net.Core.Entities.Context;
using Digital.Net.Lib.Entities.Mutations;
using Digital.Net.Core.Http.Services.Mutations.Exceptions;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using NpgsqlTypes;

namespace Digital.Net.Core.Http.Services.Mutations;

public class MutationAuditReader(
    DigitalContext context,
    IEnumerable<MutationSchema> schemas,
    IEnumerable<AuditedEntityType> auditedTypes
)
{
    private const string Columns =
        "\"Id\", \"CreatedAt\", \"UpdatedAt\", \"ChangeType\", \"EntityType\", \"EntityId\", " +
        "\"UserId\", \"IpAddress\", \"UserAgent\", \"Payload\"";

    private static readonly Dictionary<string, string> OrderColumns = new(StringComparer.OrdinalIgnoreCase)
    {
        ["CreatedAt"] = "\"CreatedAt\"",
        ["EntityType"] = "\"EntityType\"",
        ["ChangeType"] = "\"ChangeType\""
    };

    public async Task<MutationAuditPage> SearchAsync(MutationAuditCriteria criteria, CancellationToken ct)
    {
        ThrowIfRestrictedRequest(criteria);

        var schemaNames = schemas.Select(s => s.Name).Distinct().ToList();
        var allowedTypes = auditedTypes
            .Where(t => criteria.IncludeRestricted || !t.Restricted)
            .Select(t => t.Name)
            .Distinct()
            .ToArray();

        var (union, parameters) = BuildUnion(criteria, schemaNames, allowedTypes);
        var orderColumn = OrderColumns.GetValueOrDefault(criteria.OrderBy ?? string.Empty, "\"CreatedAt\"");
        var direction = criteria.Descending ? "DESC" : "ASC";
        parameters.Add(new NpgsqlParameter("size", NpgsqlDbType.Integer) { Value = criteria.Size });
        parameters.Add(new NpgsqlParameter("offset", NpgsqlDbType.Bigint)
        {
            Value = (long)(criteria.Index - 1) * criteria.Size
        });

        var rows = await context.Database
            .SqlQueryRaw<MutationAuditRow>(
                $"SELECT *, COUNT(*) OVER()::int AS \"TotalCount\" FROM (\n{union}\n) AS mutations\n" +
                $"ORDER BY {orderColumn} {direction}, \"Id\" {direction}\nLIMIT @size OFFSET @offset",
                parameters.ToArray()
            )
            .ToListAsync(ct);

        var total = rows.Count > 0 ? rows[0].TotalCount : 0;
        return new MutationAuditPage(rows, total);
    }

    private void ThrowIfRestrictedRequest(MutationAuditCriteria criteria)
    {
        if (criteria.IncludeRestricted || string.IsNullOrEmpty(criteria.EntityType)) return;
        if (auditedTypes.Any(t =>
                t.Restricted
                && t.Name.Equals(criteria.EntityType, StringComparison.OrdinalIgnoreCase)
            ))
            throw new RestrictedAuditEntityException(criteria.EntityType);
    }

    private static (string Union, List<object> Parameters) BuildUnion(
        MutationAuditCriteria criteria,
        List<string> schemaNames,
        string[] allowedTypes
    )
    {
        var filters = new List<string>();
        var parameters = new List<object>();

        // Whitelist of audited types: untracked / unknown / (non-admin) restricted rows never leave the DB.
        filters.Add("\"EntityType\" = ANY(@allowedTypes)");
        parameters.Add(new NpgsqlParameter("allowedTypes", NpgsqlDbType.Array | NpgsqlDbType.Text)
        {
            Value = allowedTypes
        });

        if (!string.IsNullOrEmpty(criteria.EntityType))
        {
            filters.Add("\"EntityType\" = @entityType");
            parameters.Add(new NpgsqlParameter("entityType", NpgsqlDbType.Text) { Value = criteria.EntityType });
        }

        if (criteria.EntityId is { } entityId)
        {
            filters.Add("\"EntityId\" = @entityId");
            parameters.Add(new NpgsqlParameter("entityId", NpgsqlDbType.Uuid) { Value = entityId });
        }

        if (criteria.UserId is { } userId)
        {
            filters.Add("\"UserId\" = @userId");
            parameters.Add(new NpgsqlParameter("userId", NpgsqlDbType.Uuid) { Value = userId });
        }

        if (criteria.ChangeType is { } changeType)
        {
            filters.Add("\"ChangeType\" = @changeType");
            parameters.Add(new NpgsqlParameter("changeType", NpgsqlDbType.Integer) { Value = changeType });
        }

        if (criteria.CreatedFrom is { } createdFrom)
        {
            filters.Add("\"CreatedAt\" >= @createdFrom");
            parameters.Add(new NpgsqlParameter("createdFrom", NpgsqlDbType.TimestampTz) { Value = AsUtc(createdFrom) });
        }

        if (criteria.CreatedTo is { } createdTo)
        {
            filters.Add("\"CreatedAt\" <= @createdTo");
            parameters.Add(new NpgsqlParameter("createdTo", NpgsqlDbType.TimestampTz) { Value = AsUtc(createdTo) });
        }

        var where = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : string.Empty;
        var union = string.Join(
            "\nUNION ALL\n",
            schemaNames.Select(schema =>
                $"SELECT {Columns}, '{schema}' AS \"Source\" FROM {schema}.\"EntityMutation\" {where}")
        );
        return (union, parameters);
    }

    // Npgsql rejects TimestampTz parameters with an Unspecified kind.
    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}