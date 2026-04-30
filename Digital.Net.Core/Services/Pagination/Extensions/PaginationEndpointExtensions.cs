using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.Models;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Services.Pagination.Extensions;

/// <summary>
///     Extension methods to map pagination endpoints for entities to Minimal API routes.
/// </summary>
public static class PaginationEndpointExtensions
{
    /// <summary>
    ///     Maps a GET endpoint to retrieve a paginated list of entities with optional filtering,
    ///     using a generic <typeparamref name="TContext" /> database context.
    /// </summary>
    /// <typeparam name="TContext">The DbContext type to use</typeparam>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TDto">The DTO type to return</typeparam>
    /// <typeparam name="TQuery">The query type containing pagination and filter parameters</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the pagination endpoint</param>
    /// <param name="filter">Optional function to add custom filters to the query predicate</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapPaginationGet<TContext, T, TDto, TQuery>(
        this IEndpointRouteBuilder app,
        string? route = null,
        Func<Expression<Func<T, bool>>, TQuery, Expression<Func<T, bool>>>? filter = null
    )
        where TContext : DbContext
        where T : Entity
        where TDto : class
        where TQuery : Query =>
        app
            .MapGet(route ?? "", (
                [AsParameters]
                TQuery query,
                TContext context
            ) =>
            {
                query.ValidateParameters();
                var result = new QueryResult<TDto>();

                try
                {
                    var predicate = BuildFilter(query, filter);
                    var items = context.Set<T>().Where(predicate);
                    var rowCount = items.Count();

                    var config = new ParsingConfig { IsCaseSensitive = false };
                    var orderBy = string.IsNullOrWhiteSpace(query.OrderBy) ? "CreatedAt" : query.OrderBy;
                    var direction = string.Equals(query.Order, "desc", StringComparison.OrdinalIgnoreCase) ? " descending" : "";

                    items = items.AsNoTracking();
                    items = items.OrderBy(config, orderBy + direction);
                    items = items.Skip((query.Index - 1) * query.Size).Take(query.Size);

                    result.Value = Mapper.TryMap<T, TDto>(items.ToList());
                    result.Total = rowCount;
                    result.Index = query.Index;
                    result.Size = query.Size;
                }
                catch (Exception e)
                {
                    result.AddError(e);
                }

                return TypedResults.Ok(result);
            })
            .WithSummary("GetPaginated")
            .WithDescription(
                "Retrieves a paginated list of entities based on the provided query parameters. Returns a QueryResult containing the paginated entities and metadata."
            );

    private static Expression<Func<T, bool>> BuildFilter<T, TQuery>(
        TQuery query,
        Func<Expression<Func<T, bool>>, TQuery, Expression<Func<T, bool>>>? filter
    )
        where T : Entity
        where TQuery : Query
    {
        var predicate = PredicateBuilder.New<T>();
        if (query.CreatedFrom.HasValue)
            predicate = predicate.Add(x => x.CreatedAt >= query.CreatedFrom);
        if (query.UpdatedFrom.HasValue)
            predicate = predicate.Add(x => x.UpdatedAt >= query.UpdatedFrom);
        if (query.CreatedTo is not null)
            predicate = predicate.Add(x => x.CreatedAt <= query.CreatedTo);
        if (query.UpdatedTo is not null)
            predicate = predicate.Add(x => x.UpdatedAt <= query.UpdatedTo);
        if (filter is not null)
            predicate = filter(predicate, query);
        return predicate;
    }
}