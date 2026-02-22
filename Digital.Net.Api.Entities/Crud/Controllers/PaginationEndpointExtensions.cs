using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Api.Core.Models;
using Digital.Net.Api.Core.Predicates;
using Digital.Net.Api.Entities.Models;
using Digital.Net.Api.Entities.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    /// <typeparam name="TContext">The DbContext type</typeparam>
    /// <typeparam name="TDto">The DTO type to return</typeparam>
    /// <typeparam name="TQuery">The query type containing pagination and filter parameters</typeparam>
    /// <param name="app">The endpoint route builder</param>
    /// <param name="route">The base route for the pagination endpoint</param>
    /// <param name="filter">Optional function to add custom filters to the query predicate</param>
    /// <returns>A RouteHandlerBuilder for further configuration</returns>
    public static RouteHandlerBuilder MapPaginationGet<T, TContext, TDto, TQuery>(
        this IEndpointRouteBuilder app,
        string route,
        Func<Expression<Func<T, bool>>, TQuery, Expression<Func<T, bool>>>? filter = null
    )
        where T : Entity
        where TContext : DbContext
        where TDto : class
        where TQuery : Query =>
        app.MapGet($"{route}", (
            [FromQuery]
            TQuery query,
            IRepository<T, TContext> repository
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

        if (query.CreatedAt.HasValue)
            predicate = predicate.Add(x => x.CreatedAt >= query.CreatedAt);
        if (query.UpdatedAt.HasValue)
            predicate = predicate.Add(x => x.UpdatedAt >= query.UpdatedAt);
        if (query.CreatedIn is not null)
            predicate = predicate.Add(x => x.CreatedAt >= query.CreatedIn.From && x.CreatedAt <= query.CreatedIn.To);
        if (query.UpdatedIn is not null)
            predicate = predicate.Add(x => x.UpdatedAt >= query.UpdatedIn.From && x.UpdatedAt <= query.UpdatedIn.To);

        if (filter is not null)
            predicate = filter(predicate, query);

        return predicate;
    }
}