namespace Digital.Net.Core.Entities.Models;

/// <summary>
///     Marks a tracked <see cref="Entity" /> whose mutations are only visible to administrators:
///     excluded from the audit API for everyone else.
/// </summary>
public interface IRestrictedAuditEntity;