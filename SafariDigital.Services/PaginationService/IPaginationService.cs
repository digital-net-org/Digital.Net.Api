using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Services.PaginationService;

public interface IPaginationService<TModel, in TQuery> where TModel : class where TQuery : PaginationQuery
{
    Task<PaginationResult<TModel>> Get(TQuery query);
}