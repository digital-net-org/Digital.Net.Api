using SafariDigital.Core.Validation;

namespace SafariDigital.Services.CrudService;

public interface ICrudService<T, TModel, in TQuery>
{
    Task<Result<TModel>> GetById(string id);
}