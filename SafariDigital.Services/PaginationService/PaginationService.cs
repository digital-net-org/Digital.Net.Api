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
    public PaginationResult<TModel> Get(TQuery query)
    {
        query.ValidateParameters();
        var result = new PaginationResult<TModel>();

        try
        {
            var items = repositoryService.Get(Filter(query));
            var rowCount = items.Count();
            items = items.AsNoTracking();
            items = items.Skip((query.Index - 1) * query.Size).Take(query.Size);
            items = items.OrderBy(query.OrderBy ?? "CreatedAt");
            result.Result = items.ToList().GetModel<TModel>();
            result.Total = rowCount;
            result.Index = query.Index;
            result.Size = query.Size;
        }
        catch (Exception e)
        {
            result.AddError(e);
        }

        return result;
    }

    protected abstract Expression<Func<T, bool>> Filter(TQuery query);
}