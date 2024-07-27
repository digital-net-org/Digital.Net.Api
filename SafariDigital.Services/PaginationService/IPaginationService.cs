using SafariDigital.Services.PaginationService.Models;

namespace SafariDigital.Services.PaginationService;

public interface IPaginationService<TModel, in TQuery> where TModel : class where TQuery : PaginationQuery
{
    PaginationResult<TModel> Get(TQuery query);
}