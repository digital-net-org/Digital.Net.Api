using SafariDigital.Core.Application;
using SafariDigital.Core.Validation;
using SafariDigital.Database.Models;
using SafariDigital.Database.Repository;
using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Services.CrudService;

public class CrudService<T, TModel, TQuery>(
    IRepositoryService<T> repositoryService
) : ICrudService<T, TModel, TQuery> where T : BaseEntity where TModel : class where TQuery : PaginationQuery
{
    public async Task<Result<TModel>> GetById(string id)
    {
        var result = new Result<TModel>();
        var queryResult = await result.ValidateExpressionAsync(repositoryService.GetByPrimaryKeyAsync(id));
        result.Value = queryResult?.GetModel<TModel>();
        return result.Value is null ? result.AddError(EApplicationMessage.QueryError) : result;
    }
}