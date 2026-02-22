using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Core.OpenApi;
using Digital.Net.Api.Core.Predicates;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Api.Entities.Crud.Controllers;

/// <summary>
///     Extension methods to map pagination endpoints for entities to Minimal API routes.
/// </summary>
public static class PaginationEndpointExtensions
{
    /// <summary>
    ///     Maps a GET endpoint to retrieve a paginated list of entities with optional filtering.
    /// </summary>
    /// <typeparam name="T">The entity type</typeparam>
    /// <typeparam name="TDto">The DTO type to return</typeparam>
    /// <typeparam name="TQuery">The query type containing pagination and filter parameters</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the pagination endpoint</param>
    /// <param name="filter">Optional function to add custom filters to the query predicate</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapPaginationGet<T, TDto, TQuery>(
        this IEndpointRouteBuilder app,
        string route,
        Func<Expression<Func<T, bool>>, TQuery, Expression<Func<T, bool>>>? filter = null
    )
        where T : Entity
        where TDto : class
        where TQuery : Query =>
        app
            .MapGet($"{route}", (
                [AsParameters]
                TQuery query,
                IRepository<T> repository
            ) =>
            {
                query.ValidateParameters();
                var result = new QueryResult<TDto>();

                try
                {
                    var predicate = BuildFilter(query, filter);
                    var items = repository.Get(predicate);
                    var rowCount = items.Count();

                    items = items.AsNoTracking();
                    items = items.Skip((query.Index - 1) * query.Size).Take(query.Size);
                    items = items.OrderBy(query.OrderBy ?? "CreatedAt");

                    result.Value = Mapper.TryMap<T, TDto>(items.ToList());
                    result.Total = rowCount;
                    result.Index = query.Index;
                    result.Size = query.Size;
                }
                catch (Exception e)
                {
                    result.AddError(e);
                }

                return Results.Ok(result);
            })
            .WithDoc(d =>
            {
                d.Summary = "GetPaginated";
                d.Description =
                    "Retrieves a paginated list of entities based on the provided query parameters. Returns a QueryResult containing the paginated entities and metadata.";
            });

    /// <summary>
    ///     Builds the filter expression for the query, combining base filters with custom filters.
    /// </summary>
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