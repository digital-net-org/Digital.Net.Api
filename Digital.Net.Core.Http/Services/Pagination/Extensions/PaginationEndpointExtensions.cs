using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Digital.Net.Core.Entities.Models;
using Digital.Net.Lib.Models;
using Digital.Net.Lib.Predicates;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Digital.Net.Core.Http.Services.Pagination.Extensions;

public static class PaginationEndpointExtensions
{
    public static RouteHandlerBuilder MapPaginationGet<TContext, T, TDto, TQuery>(
        this IEndpointRouteBuilder app,
        string? route = null,
        Func<Expression<Func<T, bool>>, TQuery, Expression<Func<T, bool>>>? filter = null,
        Expression<Func<T, object?>>[]? include = null
    )
        where TContext : DbContext
        where T : Entity
        where TDto : class
        where TQuery : Query =>
        app
            .MapGet(route ?? "", async (
                [AsParameters]
                TQuery query,
                TContext context,
                CancellationToken ct
            ) =>
            {
                query.ValidateParameters();
                var result = new QueryResult<TDto>();

                try
                {
                    var predicate = BuildFilter(query, filter);
                    var items = context.Set<T>().Where(predicate);
                    var rowCount = await items.CountAsync(ct);

                    var config = new ParsingConfig { IsCaseSensitive = false };
                    var orderClause = OrderByResolver.ResolveOrderClause<T>(query.OrderBy, query.Order);

                    items = items.AsNoTracking();
                    items = items.OrderBy(config, orderClause);
                    items = items.Skip((query.Index - 1) * query.Size).Take(query.Size);
                    if (include is not null)
                        items = include.Aggregate(items, (current, nav) => current.Include(nav));

                    result.Value = Mapper.TryMap<T, TDto>(await items.ToListAsync(ct));
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
            .WithSummary($"GetPaginated: {typeof(T).Name}")
            .WithDescription(
                $"Retrieves a paginated list of {typeof(T).Name}s based on the provided query parameters. " +
                $"Returns a QueryResult containing the paginated {typeof(T).Name}s and metadata."
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