using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using SafariDigital.Database.Models;
using SafariDigital.Database.Repository;
using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Services.PaginationService;

public abstract class PaginationService<T, TModel, TQuery>(IRepository<T> repositoryService)
    : IPaginationService<TModel, TQuery>
    where T : BaseEntity
    where TModel : class
    where TQuery : PaginationQuery
{
    public async Task<PaginationResult<TModel>> Get(TQuery query)
    {
        query.ValidateParameters();

        var items = repositoryService.Get(Filter(query));
        var rowCount = await items.CountAsync();
        items = items.AsNoTracking();
        items = items.Skip((query.Index - 1) * query.Size).Take(query.Size);
        items = items.OrderBy(query.OrderBy ?? "CreatedAt");
        var result = items.ToList().GetModel<TModel>();

        return new PaginationResult<TModel>
        {
            Items = result,
            Index = query.Index,
            Size = query.Size,
            Total = rowCount
        };
    }

    protected abstract Expression<Func<T, bool>> Filter(TQuery query);
}