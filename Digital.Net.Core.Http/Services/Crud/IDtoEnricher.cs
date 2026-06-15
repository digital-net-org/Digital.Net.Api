using Digital.Net.Lib.Entities.Models;

namespace Digital.Net.Core.Http.Services.Crud;

/// <summary>
///     Post-mapping enrichment hook for a CRUD DTO. Implementations are resolved from DI
///     <para>
///         Use this to hydrate fields that the primary <c>DbContext</c> cannot resolve on its own:
///         cross-context joins, external services, computed aggregates, etc.
///     </para>
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TDto"></typeparam>
public interface IDtoEnricher<in T, in TDto>
    where T : Entity
    where TDto : class
{
    Task EnrichAsync(TDto dto, CancellationToken ct);
}