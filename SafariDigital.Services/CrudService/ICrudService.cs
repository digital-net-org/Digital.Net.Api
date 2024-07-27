using SafariDigital.Core.Validation;
using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Services.CrudService;

public interface ICrudService<T, TModel, in TQuery>
{
    PaginationResult<TModel> Get(TQuery query);
    Task<Result<TModel>> GetById(string id);
}